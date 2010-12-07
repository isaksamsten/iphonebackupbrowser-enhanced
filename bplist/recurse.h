//
//
// Auteur: René DEVICHI
// Copyright (c) Septembre 2000
//

#ifndef recurse_h_included
#define recurse_h_included


// recurse.cpp
//
// la fonction retourne 0 pour arrêter l'énumération, 1 pour la continuer
typedef int (* recurse_fn)(LPCTSTR, void *);

int recurse(LPCTSTR initial, LPCTSTR mask,
			 recurse_fn fn, LPVOID param,
			 int recursif = 0,
			 recurse_fn fn2 = NULL, LPVOID param2 = NULL);

int recurse1(LPCTSTR path, recurse_fn fn, LPVOID param, int recursif);



#endif