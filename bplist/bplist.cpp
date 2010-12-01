// Lit les binary property list d'Apple
//
// Infos: http://paste.lisp.org/display/52154

#include "stdafx.h"


#include "base64.h"
#include "prop.h"

#include "recurse.h"
#include "endian.h"
#include "timing.h"


#define MAGIC "bplist"
#define FORMAT "00"
#define TRAILER_SIZE (sizeof( uint8_t ) * 2 + sizeof( uint64_t ) * 3)


class BPListReader
{
	BPListReader(const BPListReader&);
	void operator=(const BPListReader&);


	uint8_t		_unused[6];				// juste pour faire joli...
	uint8_t		_offsetIntSize;			// taille des éléments de la table des offsets
	uint8_t		_offsetRefSize;			// taille des index de référence (array, dict)
	uint64_t	_numObjects;			// nombre d'objets dans la PropertyList
	uint64_t	_topObject;				// index de l'objet de base
	uint64_t	_offsetTableOffset;		// offset de la table des offsets des objets (numObjects éléments)

	std::vector<uint64_t> _tableOffset;	// table des offets des objets

	__int64		_length;		// taille en octets du bplist à analyser
	__int64		_start;			// position dans le buffer OU offset de début dans le fichier
	FILE		*_file;			// fichier sur le bplist ou NULL
	std::string	_buffer;		// buffer sur le bplist si _file==NULL

	bool		_dump;

	inline const uint8_t *ptr(__int64 advance = 0)
	{
		assert(_start + advance <= _length);
		const uint8_t *p = reinterpret_cast<const uint8_t *>(_buffer.data() + _start);
		_start += advance;
		return p;
	}

	inline void setpos(__int64 position)
	{
		if (_file) {
			bool ok = _fseeki64(_file, _start + position, SEEK_SET) == 0;
			assert(ok);
		}
		else {
			assert(position >= 0 && position <= _length);
			_start = position;
		}
	}


private:
		
	inline void get(uint8_t& i)
	{
		if (_file) {
			fread(&i, 1, 1, _file);
		}
		else {
			i = *ptr(1); 
		}
	}


	inline void get(uint64_t& i)
	{
		uint8_t b;

		i = 0;
		for (size_t k = 0; k < 8; ++k) {
			get(b);
			i |= b << (56 - k * 8);
		}	
	}


	inline void get1(uint64_t& i)
	{
		uint8_t b;
		get(b);
		i = b;
	}

	
	inline void get2(uint64_t& i)
	{
		struct {
			uint8_t b0;
			uint8_t b1;
		} w;
		
		assert(sizeof(w) == 2);

		if (_file) {
			fread(&w, 1, 2, _file);
		}
		else {
			memcpy(&w, ptr(2), 2);
		}

		i = (w.b0 << 8) | (w.b1);
	}


	inline void get3(uint64_t& i)
	{
		struct {
			uint8_t b0;
			uint8_t b1;
			uint8_t b2;
		} w;

		assert(sizeof(w) == 3);
		
		if (_file) {
			fread(&w, 1, 3, _file);
		}
		else {
			memcpy(&w, ptr(3), 3);
		}

		i = (w.b0 << 16) | (w.b1 << 8) | (w.b2);
	}


	template<typename INT_TYPE>
	inline void get(INT_TYPE& i, size_t bytesCount)
	{
		uint8_t b;

		i = 0;
		for (size_t k = 0; k < bytesCount; ++k) {
			get(b);
			i |= ((INT_TYPE)b) << ((bytesCount - 1 - k) * 8);
		}	
	}


	void get(std::string& s, size_t length)
	{
		s.resize(length);
		if (_file) {
			fread(const_cast<char *>(s.data()), 1, length, _file);
		}
		else {
			memmove(const_cast<char *>(s.data()), ptr(length), length);			
		}
	}


	prop readObjectAtOffset(uint64_t atOffset, size_t indent = 0)
	{	
		setpos(atOffset);
		return readObject(indent);
	}


	prop readObjectAtRef(uint64_t ref, size_t indent = 0)
	{
		assert(ref < _numObjects);
		return readObjectAtOffset(_tableOffset[ (size_t) ref ], indent);
	}


	prop readObject(size_t indent = 0)
	{
		bool dump = _dump && (indent != -1);

		if (dump) {
			__int64 pos = _file ? (_ftelli64(_file) - _start) : _start;

			std::vector<uint64_t>::const_iterator i;
			size_t k = 0;
			for (i = _tableOffset.begin(); i != _tableOffset.end(); ++i, ++k) {
				if (*i == pos) break;
			}

			printf("offset %5I64d ",  pos);
			if (i != _tableOffset.end()) printf("(%4u) ", k);
			printf(": ");

			for (size_t i = indent; i; --i) printf("  ");
		}
		else {
			indent = -2;
		}


		// le tag de l'élément
		// 
		uint8_t marker;

		get(marker);


		//
		// les types simples
		//
		if (marker == kValueNull) {		// 0x00
			if (dump) printf("NULL\n");
			return prop::Null;
		}

		if (marker == kValueFalse) {	// 0x08
			if (dump) printf("FALSE\n");
			return prop::False;
		}

		if (marker == kValueTrue) {		// 0x09
			if (dump) printf("TRUE\n");
			return prop::True;
		}


		//
		// les objets
		//
		uint8_t topNibble = marker & 0xF0;
		uint8_t botNibble = marker & 0x0F;

		if (topNibble == kTagInt) {		// 0x1
			uint64_t number;
			get(number, 1 << botNibble);
			if (dump) { printf("integer: %I64u\n", number); }
			return prop(number);
		}
															 
		if (topNibble == kTagReal) {	// 0x2

			switch (1 << botNibble) {
				case 8: 
					{
						uint64_t number;
						get(number, 1 << botNibble);
						if (dump) { printf("real (%d): %lf\n", (int)(1 << botNibble), *(double *)&number); }
						return prop( *(double *)&number );	
					}
					break;

				case 4:
					{
						uint32_t number;
						get(number, 1 << botNibble);
						if (dump) { printf("real (%d): %f\n", (int)(1 << botNibble), *(float *)&number); }
						return prop(*(float *)&number);	
					}
					break;

				default:
					printf("ERROR: real length unsupported\n");
					assert(0);
					throw std::exception("real length unsupported");
			}
		}
			
		if (topNibble == kTagDate) {		// 0x3
			switch (1 << botNibble) {
				case 8: 
					{
						uint64_t number;
						get(number, 1 << botNibble);

						if (dump) printf("date (%d): %lf\n", (int)(1 << botNibble), *(double *)&number);
											
						return prop::createDate(*(double *)&number);
					}
					break;

				default:
					printf("ERROR: date length unsupported\n");
					assert(0);
					throw std::exception("date length unsupported");
			}	
		}


		if (topNibble == kTagData || topNibble == kTagASCIIString || topNibble == kTagUnicodeString
			|| topNibble == kTagUID || topNibble == kTagArray || topNibble == kTagDictionary)
		{
			uint64_t length;

			if (botNibble != 0xF) {
				length = botNibble;
			}
			else {
				prop p = readObject(-1);
				length = p.toInteger();
			}


			if (topNibble == kTagData) {
				prop p;

				get(p.initData(), (size_t) length);

				if (dump) printf("data (%I64u)\n", length);

				return p;
			}


			if (topNibble == kTagASCIIString) {
				std::string s;
				get(s, (size_t) length);

				prop p(s);

				if (dump) printf("string (%I64u): %s\n", length, s.c_str());

				return p;
			}
			

			if (topNibble == kTagUnicodeString) {
				std::wstring utf16;

				wchar_t c;
				for (size_t k = 0; k < length; ++k) {
					get(c, 2);
					utf16.append(1, c);
				}

				prop p(utf16);

				if (dump) {
					printf("unicode (%I64u): %ws", length, utf16.c_str());
					printf("\n");
				}

				return p;
			}


			if (topNibble == kTagUID) {
				uint64_t number;
				get(number, 1 << botNibble);
				if (dump) printf("UID (%d): %I64d\n", 1 << botNibble, number);

				prop p;
				p.initUID(number);
				return p;
			}					


			if (topNibble == kTagArray) {
				if (dump) printf("array (%I64u elements)\n", length);

				prop	p;

				prop::TArray& array = p.initArray();

				std::vector<uint64_t> values;
				uint64_t objRef;
		
				for (size_t k = 0; k < length; ++k) {
					get(objRef, _offsetRefSize);				
					values.push_back(objRef);
				}

				for (size_t k = 0; k < length; ++k) {
					prop value = readObjectAtRef(values[k], indent + 1);			
					array.push_back(value);
				}

				return p;
			}


			if (topNibble == kTagDictionary)
			{
				if (dump) printf("dictionary (%I64u elements)\n", length);

				prop	p;

				prop::TDict& dict = p.initDict();

				std::vector<uint64_t> keys;
				std::vector<uint64_t> values;

				uint64_t objRef;
		
				for (size_t k = 0; k < length; ++k) {
					get(objRef, _offsetRefSize);				
					keys.push_back(objRef);				
				}

				for (size_t k = 0; k < length; ++k) {
					get(objRef, _offsetRefSize);				
					values.push_back(objRef);				
				}

				for (size_t k = 0; k < length; ++k) {
					prop key = readObjectAtRef(keys[k], indent + 1);
					prop value = readObjectAtRef(values[k], indent + 1);
				
					dict[key.toString()] = value;
				}

				return p;
			}
		}

		printf("unknown marker 0x%02X\n", marker);
			
		return prop::Null;
	}


	bool init()
	{
		// get trailer
		setpos(_length - TRAILER_SIZE);
		get(_offsetIntSize);
		get(_offsetRefSize);
		get(_numObjects);
		get(_topObject);
		get(_offsetTableOffset);

		// get the offset table
		_tableOffset.resize((size_t) _numObjects);
		setpos(_offsetTableOffset);

		// optimisation
		switch (_offsetIntSize) {
		case 1:
			for (size_t k = 0; k < _numObjects; ++k) {
				get1(_tableOffset[k]);
			}
			break;
		
		case 2:
			for (size_t k = 0; k < _numObjects; ++k) {
				get2(_tableOffset[k]);
			}
			break;

		case 3:
			for (size_t k = 0; k < _numObjects; ++k) {
				get3(_tableOffset[k]);
			}
			break;
		
		default:
			for (size_t k = 0; k < _numObjects; ++k) {
				get(_tableOffset[k], _offsetIntSize);
			}
		}

		if (_dump) {
			printf("Offsets are %u bytes\n", (unsigned) _offsetIntSize);
			printf("Object Refs are %u bytes\n", (unsigned) _offsetRefSize);
			printf("There are %I64u objects in the file\n", _numObjects);
			printf("The top object is at %I64u\n", _topObject);
			printf("The Offset Table is at offset %I64u\n", _offsetTableOffset);

			std::vector<uint64_t>::const_iterator k;
			printf("Offset table: {");
 			for (k = _tableOffset.begin(); k != _tableOffset.end(); ++k) {
				printf(" %I64u", *k);
			}
			printf(" }\n\n");
		}

		return true;
	}

public:
	BPListReader(bool dump = false) : _file(0), _dump(dump)
	{
	}

	virtual ~BPListReader()
	{
		close();
	}


	void close()
	{
		if (_file) {
			fclose(_file);
			_file = 0;
		}

		_buffer.clear();

		_length = 0;
		_start = 0;
	}


	bool open(const std::string& buffer)
	{
		close();

		_buffer = buffer;
		_start = 0;
		_length = buffer.length();

		// longueur de l'entête + trailer
		if (_length < 8 + TRAILER_SIZE)
			return false;

		// la signature
		if (memcmp(_buffer.data(), "bplist", 6) != 0)
			return false;

		// la version
		if (memcmp(_buffer.data() + 6, "00", 2) != 0)
			return false;

		return init();
	}


	bool open(const void *buffer, size_t length)
	{
		close();

		_buffer.assign(static_cast<const char *>(buffer), length);
		_start = 0;
		_length = length;

		// longueur de l'entête + trailer
		if (_length < 8 + TRAILER_SIZE)
			return false;

		// la signature
		if (memcmp(_buffer.data(), "bplist", 6) != 0)
			return false;

		// la version
		if (memcmp(_buffer.data() + 6, "00", 2) != 0)
			return false;

		return init();
	}


	bool open(FILE *fh, __int64 start, __int64 length)
	{	
		close();

		_file = _fdopen(_dup(_fileno(fh)), "rb");
		_start = start;
		_length = length;
	
		char magic[8];

		_fseeki64(_file, _start, SEEK_SET);

		if (fread(magic, 1, 8, _file) != 8)
			return false;

		// la signature
		if (strncmp(magic, "bplist", 6) != 0)
			return false;

		// la version
		if (strncmp(magic + 6, "00", 2) != 0)
			return false;

		return init();
	}


	prop getRootObject()
	{
		if (!_file && _buffer.empty())
			return prop::Null;

		return readObjectAtOffset(_tableOffset[(size_t) _topObject]);
	}


	bool open(const std::wstring& filename)
	{
		FILE *f;
		bool ret = false;

		if (_wfopen_s(&f, filename.c_str(), L"rb") == 0) {

			_fseeki64(f, 0, SEEK_END);
			ret = open(f, 0, _ftelli64(f));
			fclose(f);
		}

		return ret;
	}

};



int analyse_mdinfo(const wchar_t *mdinfo, void *)
{
	printf("-----------------------------------------------------------------\n");
	printf("File: %ls\n", mdinfo);

	BPListReader r;

	if (r.open(mdinfo)) {

		prop p = r.getRootObject();

		prop Metadata = p.select(L"Metadata");

		std::string buffer;

		
		p.toXmlDoc(std::cout);


		if (Metadata.toData(buffer)) {

			BPListReader rr;

			rr.open(buffer);
			
			rr.getRootObject().toXmlDoc(std::cout);

			prop Metadata = rr.getRootObject();

			std::wstring path = Metadata.select("Domain").toString() + L"/" + Metadata.select("Path").toString();

			std::wcout << L"Path: " << path << std::endl;
		}
	}

	return 1;
}


struct mdinfo
{
	std::wstring Domain;
	std::wstring Path;
	bool Greylist;
	int Version;
};


int analyse_mdinfo2(const wchar_t *path, void *ptr)
{
	std::list<mdinfo>& infos = *static_cast<std::list<mdinfo> *>(ptr);

	BPListReader r;

	if (r.open(path)) {

		prop p = r.getRootObject();

		prop Metadata = p.select(L"Metadata");

		std::string buffer;

		if (Metadata.toData(buffer)) {

			BPListReader rr;

			rr.open(buffer);

			prop Metadata = rr.getRootObject();

			mdinfo mi;

			mi.Domain = Metadata.select("Domain").toString();
			mi.Path   = Metadata.select("Path").toString();
			mi.Greylist = Metadata.select("Greylist").toBoolean();			

			std::wstring v = Metadata.select("Version").toString();
			wchar_t *e = 0;
			mi.Version = 100 * wcstoul(v.c_str(), &e, 10);
			if (e && *e == L'.')
				mi.Version += wcstoul(e + 1, 0, 10);

			infos.push_back(mi);
		}
	}

	return 1;
}


int analyse_mddata(const wchar_t *mddata, void *)
{
	std::wcout << "-----------------------------------------------------------------" << std::endl;
	std::wcout << "File: " << mddata << std::endl;

	BPListReader r;
	prop p;
	std::wstring path;

	std::wstring mdinfo(mddata);

	if (mdinfo.length() < wcslen(L".mddata")) return 1;
	if (_wcsicmp(mdinfo.data() + mdinfo.length() - wcslen(L".mddata"), L".mddata") != 0) return 1;
	mdinfo.resize(mdinfo.length() - wcslen(L".mddata"));
	mdinfo.append(L".mdinfo");

	if (r.open(mdinfo)) {
		p = r.getRootObject().select(L"Metadata");

		std::string buffer;

		if (p.toData(buffer)) {

			BPListReader r;
			prop p;

			r.open(buffer);

			p = r.getRootObject();

			path = p.select("Domain").toString() + L"/" + p.select("Path").toString();

			std::wcout << "Domain: " << p.select("Domain").toString() << std::endl
			           << "Path:   " << p.select("Path").toString() << std::endl;
		}
		else {
			std::wcerr << L"ERROR" << std::endl;
		}
	}
	else {
		std::wcerr << L"ERROR" << std::endl;
	}


	if (r.open(mddata)) {
		if (path.empty()) {
			r.getRootObject().toXmlDoc(std::cout);
		}
		else {
			std::wstring dir;
			size_t p = 0, q;
			while ((q = path.find(L'/', p)) != path.npos) {
				std::wstring dir = path.substr(0, q);
				_wmkdir(dir.c_str());
				p = q + 1;
			}

			path += L".xml";

			//std::replace(path.begin(), path.end(), L'/', L'\\');

   			std::ofstream  f(path.c_str());

			if (f.is_open()) {
				r.getRootObject().toXmlDoc(f);
			}
			else {
				std::wcerr << L"ERROR " << path << std::endl;
			}
			f.close();
		}
	}
	else {
		if (path.empty()) {
			//rien
			1==1;
		}
		else {
			std::wstring dir;
			size_t p = 0, q;
			while ((q = path.find(L'/', p)) != path.npos) {
				std::wstring dir = path.substr(0, q);
				_wmkdir(dir.c_str());
				p = q + 1;
			}

			CopyFile(mddata, path.c_str(), TRUE);
		}
	}

	return 1;
}


int analyse(const wchar_t *filename, void *)
{
	printf("-----------------------------------------------------------------\n");
	printf("File: %ls\n", filename);

	BPListReader r;

	if (r.open(filename)) {
		r.getRootObject().toXmlDoc(std::cout);				
	}

	return 1;
}


void bench(const wchar_t *filename)
{
	timing t;

	printf("file %ls\n", filename);

	FILE *f;
	if (_wfopen_s(&f, filename, L"rb") != 0)
		return;
	fseek(f, 0, SEEK_END);
	long len = ftell(f);
	fseek(f, 0, SEEK_SET);
	std::string buf(len, 0);
	fread((char*)buf.data(), 1, len, f);
	fclose(f);

	printf("read %lf\n", t.elapsed());

	BPListReader r;

	t.restart();
	bool ok = r.open(buf);
	printf("open %lf\n", t.elapsed());

	if (ok) {
		//r.getRootObject().toXmlDoc(std::cout);

		t.restart();
		prop p = r.getRootObject();
		printf("getRootObject %lf\n", t.elapsed());

		stringbuilder out;
		
		t.restart();
		p.toXmlDoc(out, false);
		printf("toXmlDoc %lf\n", t.elapsed());

		//std::cout << out.str();
	}
}



void toXml(const wchar_t *filename)
{
	BPListReader r;

	if (r.open(filename)) {
		//r.getRootObject().toXmlDoc(std::cout);

		stringbuilder out;		
		r.getRootObject().toXmlDoc(out, true, true, false);
		std::cout << out.str();

		/*
		stringbuilderUTF16 out;
		r.getRootObject().toXmlDocUTF16(out, true, true, false);
		std::wcout << out.str();
		*/	
	}
}



void toXml2(const wchar_t *filename)
{
	FILE *f;
	if (_wfopen_s(&f, filename, L"rb") != 0)
		return;
	fseek(f, 0, SEEK_END);
	long len = ftell(f);
	fseek(f, 0, SEEK_SET);
	std::string buf(len, 0);
	fread((char*)buf.data(), 1, len, f);
	fclose(f);

	BPListReader r(false);
	if (r.open(buf)) {
		prop p = r.getRootObject();

		if (wcsstr(filename, L".mdinfo")) {

			prop Metadata = p.select(L"Metadata");
			std::string buffer;
			if (Metadata.toData(buffer)) {

				BPListReader rr;

				if (rr.open(buffer)) {

					//std::cout << "\nFile is a .mdinfo ! Metadata is:\n";

					prop p = rr.getRootObject();

					std::wstring path = p.select("Domain").toString() + L":" + p.select("Path").toString();

					/*
					std::wcout << "Domain: " << p.select("Domain").toString() << std::endl
							   << "Path:   " << p.select("Path").toString() << std::endl;
					*/

					std::wcout << path << std::endl;

					//std::wstring path = Metadata.select("Domain").toString() + L"/" + Metadata.select("Path").toString();
					//std::wcout << L"Path: " << path << std::endl;

					//p.toXml(std::cout, 0);
				}
			}
		}
		else {
			std::stringstream out;
			p.toXmlDoc(out);
			std::cout << out.str();
		}
	}
}


#ifdef _CONSOLE

int wmain(int argc, wchar_t* argv[])
{
	//SetProcessAffinityMask(GetCurrentProcess(), 0);
	//printf("SetThreadPriority %d\n", SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_TIME_CRITICAL));

	if (argc >= 2) {
		while (--argc) {
			//timing perf;

			toXml(*++argv);

			//fprintf(stderr, "%lf s\n", perf.elapsed());
		}
	}
	/*
	else {
		PWSTR	path = 0;
		std::wstring s;

		HRESULT hr = SHGetKnownFolderPath(FOLDERID_RoamingAppData, 0, NULL, &path);

		if (SUCCEEDED(hr))
			s = path;

		CoTaskMemFree(path);

		s += L"/Apple Computer/MobileSync/Backup/cb3ada07c4d12aea99a5ec86edfe4907518aa614/*.mdinfo";

		std::list<mdinfo> infos;

		recurse1(s.c_str(), analyse_mdinfo2, &infos, FALSE);

		printf("%u\n", infos.size());

//		recurse1(L"C:/temp/bplist/backup/*.mddata", analyse_mddata, NULL, FALSE);
//		recurse1(_T("C:/temp/bplist/apps/*"), analyse, NULL, TRUE);
	}
	*/

	std::cout.flush();
	std::wcout.flush();

	return 0;
}

#else

// http://msdn.microsoft.com/en-us/library/26thfadc.aspx
// http://msdn.microsoft.com/en-us/library/as6wyhwt(VS.80).aspx

static void copy(wchar_t **& out, const std::wstring& in)
{
	if (out) {
		*out = (wchar_t *) CoTaskMemAlloc(sizeof(wchar_t) * (in.length() + 1));	
		wcscpy_s(*out, in.length() + 1, in.c_str());
	}
}


static void copy(char **& out, const std::string& in)
{
	if (out) {
		*out = (char *) CoTaskMemAlloc(in.length() + 1);
		strcpy_s(*out, in.length() + 1, in.c_str());
	}
}


extern "C" __declspec(dllexport)
int __stdcall mdinfo(const wchar_t *filename, wchar_t **Domain, wchar_t **Path)
{
	BPListReader r;

	if (r.open(filename)) {

		prop p = r.getRootObject();

		prop Metadata = p.select(L"Metadata");

		std::string buffer;

		if (Metadata.toData(buffer)) {

			BPListReader rr;

			rr.open(buffer);

			prop Metadata = rr.getRootObject();

			copy(Domain, Metadata.select("Domain").toString());
			copy(Path  , Metadata.select("Path").toString());
		}
	}

	return 0;
}


extern "C" __declspec(dllexport)
int __stdcall  bplist2xml_buffer(const char *byteArray, size_t length, char **xml, bool useOpenStepEpoch)
{
	BPListReader r;

	stringbuilder ss;

	if (r.open(byteArray, length)) {
		r.getRootObject().toXmlDoc(ss, false, false, useOpenStepEpoch);
	}

	copy(xml, ss.str());
	
	return ss.str().length();
}


extern "C" __declspec(dllexport)
int __stdcall  bplist2xml_file(const wchar_t *filename, char **xml, bool useOpenStepEpoch)
{
	BPListReader r;

	stringbuilder ss;

	if (r.open(filename)) {
		r.getRootObject().toXmlDoc(ss, false, false, useOpenStepEpoch);
	}

	copy(xml, ss.str());
	
	return ss.str().length();
}


/*
extern "C" __declspec(dllexport)
int __stdcall test(const char *byteArray, size_t length, char **xml, bool useOpenStepEpoch)
{
	(void)byteArray;
	(void)length;
	(void)useOpenStepEpoch;

	FILE *f;
	if (_wfopen_s(&f, L"C:\\temp\\a.xml", L"rb") != 0)
		return 0;
	fseek(f, 0, SEEK_END);
	long len = ftell(f);
	fseek(f, 0, SEEK_SET);
	std::string buf(len, 0);
	fread((char*)buf.data(), 1, len, f);
	fclose(f);

	copy(xml, buf);

	return 0;
}
*/


#endif
