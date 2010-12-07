// stdafx.h : fichier Include pour les fichiers Include système standard,
// ou les fichiers Include spécifiques aux projets qui sont utilisés fréquemment,
// et sont rarement modifiés
//

#pragma once

#ifndef _WIN32_WINNT		// Autorise l'utilisation des fonctionnalités spécifiques à Windows XP ou version ultérieure.                   
#define _WIN32_WINNT 0x0600	// Attribuez la valeur appropriée à cet élément pour cibler d'autres versions de Windows.
#endif						

#include <windows.h>
#include <tchar.h>

#include <shlobj.h>
#include <objbase.h>

#if _WIN32_WINNT < 0x0600
#include "c:/Microsoft IDN Mitigation APIs/Include/normalization.h"
#endif


#include <stdio.h>

#include <io.h>

#include <cstring>
#include <cassert>
#include <ctime>

#include <algorithm>
#include <memory>
#include <vector>
#include <string>
#include <map>
#include <list>
#include <iostream>
#include <sstream>
#include <fstream>
