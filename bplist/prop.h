#pragma once



typedef unsigned char uint8_t;
typedef unsigned short uint16_t;
typedef unsigned long uint32_t;
typedef unsigned __int64 uint64_t;



#ifndef _HAS_TR1
#include <boost/shared_ptr.hpp>
namespace std { namespace tr1 { 
	using ::boost::shared_ptr;
} }
#endif


enum KPropType
{
	// Object tags (high nibble)
	kTagSimple			= 0x00,	// Null, true, false, filler, or invalid
	kTagInt				= 0x10,
	kTagReal			= 0x20,
	kTagDate			= 0x30,
	kTagData			= 0x40,
	kTagASCIIString		= 0x50,
	kTagUnicodeString	= 0x60,
	kTagUID				= 0x80,
	kTagArray			= 0xA0,
	kTagDictionary		= 0xD0,
	
	// "simple" object values
	kValueNull			= 0x00,
	kValueFalse			= 0x08,
	kValueTrue			= 0x09,
	kValueFiller		= 0x0F,
};


template<typename T>
std::string lexical_cast(T t);

template<>
std::string lexical_cast(uint64_t t)
{
	char buf[32];		// il y a 21 chiffres au max pour un entier 64-bit
	_ui64toa_s(t, buf, _countof(buf), 10);
	return buf;
}

template<>
std::string lexical_cast(__int64 t)
{
	char buf[32];		// il y a 21 chiffres au max pour un entier 64-bit
	_i64toa_s(t, buf, _countof(buf), 10);
	return buf;
}

template<>
std::string lexical_cast(double t)
{
	char buf[_CVTBUFSIZE];
	_gcvt_s(buf, t, 12);
	return buf;
}



class stringbuilder : std::string
{
	char buf[_CVTBUFSIZE];

public:
	const std::string& str() const
	{
		return *this;
	}

	stringbuilder& operator<<(uint64_t t)
	{
		if (_ui64toa_s(t, buf, _countof(buf), 10) == 0)
			append(buf);
		return *this;
	}

	stringbuilder& operator<<(__int64 t)
	{
		if (_i64toa_s(t, buf, _countof(buf), 10) == 0)
			append(buf);
		return *this;
	}

	stringbuilder& operator<<(double t)
	{
		if (_gcvt_s(buf, t, 12) == 0)
			append(buf);
		return *this;
	}

	stringbuilder& operator<<(const char *t)
	{
		if (t)
			append(t);
		return *this;
	}

	stringbuilder& operator<<(const std::string& t)
	{
		append(t);
		return *this;
	}

	stringbuilder& operator<<(stringbuilder& (*fn)(stringbuilder& _Ostr))
	{	// call stringbuilder manipulator
		return (*fn)(*this);
	}
};


namespace std
{
	inline stringbuilder& endl(stringbuilder& _Ostr)
	{	// insert newline and flush byte stream
		_Ostr << "\n";
		return (_Ostr);
	}
}



class prop
{
public:
	typedef std::vector<prop> TArray;
	typedef std::map<std::wstring, prop> TDict;

private:

	class object
	{
		object(const object&);
		void operator=(const object&);
	
	public:
		KPropType			tag;

		union 
		{
			__int64			integer;		// kTagInt
			double			real;			// kTagReal
			std::string		*data;			// kTagData
			std::string		*ascii;			// kTagASCIIString
			std::wstring	*unicode;		// kTagUnicodeString
			TArray			*array;			// kTagArray
			TDict			*dict;			// kTagDictionary
		};

	public:
		object() : tag(kValueNull)
		{
		}

		explicit object(bool value) 
		{
			tag = value ? kValueTrue : kValueFalse;
		}

		explicit object(KPropType type) : tag(type)
		{
			switch (tag) {
				case kTagData			: data = new std::string; break;
				case kTagASCIIString	: ascii = new std::string; break;
				case kTagUnicodeString	: unicode = new std::wstring; break;
				case kTagArray			: array = new TArray; break;
				case kTagDictionary		: dict = new TDict; break;
			}
		}

		explicit object(uint64_t value)
		{
			tag = kTagInt;
			integer = value;
		}


		explicit object(double value)
		{
			tag = kTagReal;
			real = value;
		}

		explicit object(const std::string& str)
		{
			tag = kTagASCIIString;
			ascii = new std::string(str);
		}

		explicit object(const std::wstring& str)
		{
			tag = kTagUnicodeString;
			unicode = new std::wstring(str);
		}

		~object()
		{
			switch (tag) {
				case kTagData			: delete data;		break;
				case kTagASCIIString	: delete ascii;		break;
				case kTagUnicodeString	: delete unicode;	break;		
				case kTagArray			: delete array;		break;
				case kTagDictionary		: delete dict;		break;
			}
			tag = kValueNull;
		}
	};

	std::tr1::shared_ptr<object> _value;


	static std::string toUtf8(const std::wstring& utf16)
	{
		std::string utf8;
		int len = WideCharToMultiByte(CP_UTF8, 0, utf16.data(), utf16.length(), NULL, 0, 0, 0);
		if (len != 0) { 
			utf8.resize(len);
			WideCharToMultiByte(CP_UTF8, 0, utf16.data(), utf16.length(), const_cast<char *>(utf8.data()), len, 0, 0);
		}
   
		return utf8;   
	}

	static std::wstring fromAscii(const std::string& s)
	{
		std::wstring w;
		w.resize(s.length());
		for (size_t i = 0; i < s.length(); ++i) {
			char c = s[i];
			assert(c >= 0 && c <= 127);
			w[i] = c;		// il y a équivalence entre ASCII et UTF-16 pour la plage 0-127
		}
		return w;
	}


	// constructeur utilisé pour les constantes False et True
	explicit prop(bool value) : _value(new object(value))
	{
	}

public:
	prop() //: _value(new object)
	{
	}

	// les constructeurs simples (integer/real/string)
	template<typename T>
	explicit prop(T value) : _value(new object(value))
	{
	}


	// les trois constantes simples
	static prop	Null;
	static prop	True;
	static prop	False;


	TDict& initDict()
	{
		_value.reset(new object(kTagDictionary));
		return *_value->dict;
	}

	TArray& initArray()
	{
		_value.reset(new object(kTagArray));
		return *_value->array;
	}

	std::string& initData()
	{
		_value.reset(new object(kTagData));		
		return *_value->data;
	}

	std::string& initData(const std::string& str)
	{
		_value.reset(new object(kTagData));		
		_value->data->assign(str);
		return *_value->data;
	}

	void initUID(uint64_t value)
	{
		_value.reset(new object(kTagUID));		
		_value->integer = value;
	}

	void initDate(double value)
	{
		_value.reset(new object(kTagDate));		
		_value->real = value;
	}

	static prop createDate(double value)
	{
		prop p;
		p._value.reset(new object(kTagDate));		
		p._value->real = value;
		return p;
	}

/*
	// operator pour la map<>
	bool operator<(const prop& r) const
	{		
		return toString().compare(r.toString()) < 0;
	}
*/

	std::wstring toString() const
	{
		assert(_value);

		switch (_value->tag) {
			case kTagASCIIString:
				return fromAscii(*_value->ascii);
				break;

			case kTagUnicodeString:
				{
					if (! IsNormalizedString(NormalizationC, _value->unicode->data(), _value->unicode->length())) {
						//printf("macosx ~~\n");

						std::wstring norm;
						norm.resize(_value->unicode->length());
						int l = NormalizeString(NormalizationC, _value->unicode->data(), _value->unicode->length(), 
							const_cast<wchar_t*>(norm.data()), norm.length());
						norm.resize(l);
						return norm;
					}
					
					return *_value->unicode;
				}
				break;
		}		

		assert(0);
		return L"ERROR";
	}


	uint64_t toInteger() const
	{
		assert(_value);

		switch (_value->tag) {
			case kTagInt:
				return _value->integer;
				break;
		}

		assert(0);
		return 0;
	}


	bool toBoolean() const
	{
		assert(_value);

		switch (_value->tag) {
			case kValueFalse:
				return false;
			case kValueTrue:
				return true;
		}

		assert(0);
		return false;
	}


	template<class T>
	void toXml(T& out, size_t indent = -1, bool useOpenStepEpoch = true) const
	{
		// nota: pretty <=> (indent != -1)

		assert(_value);

		static const char TAB[] = "\t";

		if (indent != -1) {
			for (size_t k = 0; k < indent; ++k) out << TAB;
		}

		switch (_value->tag) {
			case kValueFalse		: 
				out << "<false/>";	
				break;

			case kValueTrue			: 
				out << "<true/>";		
				break;

			case kTagInt			: 
				out << "<integer>" << _value->integer << "</integer>";
				break;

			case kTagReal			: 
				out << "<real>" << _value->real << "</real>";	
				break;

			case kTagASCIIString	:
				#ifdef _DEBUG
				for (size_t k = 0; k < _value->ascii->length(); ++k) {
					char c = _value->ascii->at(k);
					assert(c >= 0 && c <= 127);
				}
				#endif
				out << "<string>" << html_encode(*_value->ascii) << "</string>";
				break;

			case kTagUnicodeString  : 
				out << "<string>" << html_encode(toUtf8(*_value->unicode)) << "</string>";		
				break;

			case kTagData			:
				out << "<data>";
				out << base64_encode(*_value->data);
				out << "</data>";			
				break;

			case kTagArray:
				{
					if (_value->array->empty()) {
						out << "<array/>";
					}
					else {
						TArray::const_iterator i;
						out << "<array>";
						if (indent != -1) { out << std::endl; }
						for (i = _value->array->begin(); i != _value->array->end(); ++i) {
							i->toXml(out, indent != -1 ? (indent + 1) : -1, useOpenStepEpoch);
						}					
						if (indent != -1) { for (size_t k = 0; k < indent; ++k) out << TAB; }
						out << "</array>";
					}
				}
				break;
		
			case kTagDictionary:
				{
					TDict::const_iterator i;

					out << "<dict>";
					if (indent != -1) { out << std::endl; }

					for (i = _value->dict->begin(); i != _value->dict->end(); ++i) {
						if (indent != -1) { for (size_t k = 0; k < indent + 1; ++k) out << TAB; }
						out << "<key>" << toUtf8(i->first) << "</key>";
						if (indent != -1) { out << std::endl; }
						i->second.toXml(out, indent != -1 ? (indent + 1) : -1, useOpenStepEpoch);
					}
					
					if (indent != -1) { for (size_t k = 0; k < indent; ++k) out << TAB; }
					out << "</dict>";
				}
				break;

			case kTagUID:
				out << "<dict><key>CF$UID</key><integer>" << _value->integer << "</integer></dict>";
				break;

			case kTagDate:
				{
					// exemple:
					//		<date>1272784073.</date>
					// <date>2041-05-02T07:07:53Z</date>
					
					__time64_t t;
					struct tm tm;
					char buf[32];		// 20 + le zéro final

					t = (__int64)(_value->real);

					if (useOpenStepEpoch) {
						// the offset from the OpenStep reference date of 1 January 2001, GMT
						t += 978307200;			// http://www.epochconverter.com/epoch/timestamp-list.php
					}

					_gmtime64_s(&tm, &t);
					strftime(buf, _countof(buf), "%Y-%m-%dT%H:%M:%SZ", &tm);

					out << "<date>" << buf << "</date>";
				}
				break;

			default:
				assert(0);
			
		}

		if (indent != -1) { out << std::endl; }
	}


	template<class T>
	void toXmlDoc(T& out, bool pretty = true, bool includeDTD = true, bool useOpenStepEpoch = true) const
	{
		assert(_value);

		out << "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
		if (pretty) out << std::endl;
		if (includeDTD) {
			out << "<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">";
			if (pretty) out << std::endl;
		}
		out << "<plist version=\"1.0\">";
		if (pretty) out << std::endl;

		toXml(out, pretty ? 0 : -1, useOpenStepEpoch);

		out << "</plist>";
		if (pretty) out << std::endl;
	}

	
	prop select(const std::wstring& key) const
	{
		assert(_value);

		if (_value->tag != kTagDictionary)
			return Null;

		TDict::const_iterator k = _value->dict->find(key);

		if (k == _value->dict->end())
			return Null;

		return k->second;
	}

	prop select(const std::string& key) const
	{
		return select(fromAscii(key));
	}

	bool toData(const void *& buffer, __int64& length) const
	{
		assert(_value);

		if (_value->tag != kTagData)
			return false;

		buffer = static_cast<const void *>(_value->data->data());
		length = _value->data->length();

		return true;
	}

	bool toData(std::string& buffer) const
	{
		assert(_value);

		if (_value->tag != kTagData)
			return false;

		buffer = *_value->data;

		return true;
	}
};


prop prop::Null;
prop prop::True(true);
prop prop::False(false);
