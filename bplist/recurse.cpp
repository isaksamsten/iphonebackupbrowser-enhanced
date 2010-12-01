//
//
// Auteur: René DEVICHI
// Copyright (c) Septembre 2000
//

#include "stdafx.h"
#include "recurse.h"



#define TEST_BIT(var,flag) (((var) & flag)==(flag))


int recurse(LPCTSTR initial, LPCTSTR mask,
			 recurse_fn fn, LPVOID param,
			 int recursif,
			 recurse_fn fn2, LPVOID param2)
{
	WIN32_FIND_DATA		w32fd;
	TCHAR				path[MAX_PATH], *p;
	HANDLE				h;
	int					stop = 0;

	if (!fn)
		return 0;

	wcscpy_s(path, initial);
	for (p = path; *p; p++)
		if (*p == '/') *p = TEXT('\\');
	p--;
	if (*p != TEXT('/') && *p != TEXT('\\')) { ++p; wcscat_s(path, TEXT("\\")); }
	p++;

	std::wstring root = path;
	size_t rootlen = root.length();

	wcscat_s(path, mask);

	h = FindFirstFile(path, &w32fd);
	if (h != INVALID_HANDLE_VALUE) {

		do {

			if (TEST_BIT(w32fd.dwFileAttributes, FILE_ATTRIBUTE_OFFLINE))
				continue;

			if (TEST_BIT(w32fd.dwFileAttributes, FILE_ATTRIBUTE_DIRECTORY))
				continue;

	
			root.append(w32fd.cFileName);

			if (! fn(root.c_str(), param)) {
				stop = 1;
				break;
			}

			root.resize(rootlen);

		} while (FindNextFile(h, &w32fd));

		FindClose(h);

		if (stop)
			return 0;

	}

	//	stop = 0; // par la force des choses !

	if (recursif) {
		lstrcpy(p, TEXT("*"));

		h = FindFirstFile(path, &w32fd);
		if (h == INVALID_HANDLE_VALUE)
			return 1;

		do {

			if (TEST_BIT(w32fd.dwFileAttributes, FILE_ATTRIBUTE_OFFLINE))
				continue;

			if (TEST_BIT(w32fd.dwFileAttributes, FILE_ATTRIBUTE_DIRECTORY)) {

				if (lstrcmp(w32fd.cFileName, TEXT(".")) == 0
					|| lstrcmp(w32fd.cFileName, TEXT("..")) == 0) continue;

				if (w32fd.dwFileAttributes != 16) {
					//printf("%s %d\n", path, w32fd.dwFileAttributes);
					//DebugBreak();
				}

				lstrcpy(p, w32fd.cFileName);
				if (recurse(path, mask, fn, param, 1, fn2, param2) == 0) {
					stop = 1;
					break;
				}

				if (fn2) {
					if (! fn2(path, param2)) {
						stop = 1;
						break;
					}
				}
			}

		} while (FindNextFile(h, &w32fd));

		FindClose(h);

	}

	return 1 - stop;
}


int recurse1(LPCTSTR path, recurse_fn fn, LPVOID param, int recursif)
{
	TCHAR	buf[MAX_PATH], *p;

	if ( GetFullPathName(path, _countof(buf), buf, &p) ) {
		if (!p || p == buf)
			p = buf;
		else
			*(p - 1) = 0;
	}
	else
		*buf = 0;

	return recurse(buf, p, fn, param, recursif);
}
