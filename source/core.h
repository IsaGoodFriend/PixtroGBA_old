#ifndef _PIX_CORE
#define _PIX_CORE

#define __DEBUG__

#include <tonc.h>
#include <string.h>

#include "engine.h"

typedef struct Actor
{
	int x, y, velX, velY;
	unsigned short width, height;
	unsigned int ID;
	
	unsigned int flags;
	
	
} Actor;

extern Actor PHYS_actors[ACTOR_LIMIT];

//#define START_FADE()		0//GAME_fading = 1; GAME_fadeAmount = 0; GAME_loadIndex = 0
#define TRANSITION_CAP		16

extern unsigned int GAME_freeze, GAME_life;

extern unsigned short* transition_style;

extern unsigned int maxEntities;

extern unsigned int visualFlags;


extern unsigned int levelFlags;
extern char gamestate, nextGamestate;

#define IS_FADING 0//(GAME_fading || GAME_fadeAmount)
extern int GAME_fadeAmount, GAME_fading, GAME_loadIndex;

extern int camX, camY, prevCamX, prevCamY;

extern void (*custom_update)(void);
extern void (*custom_render)(void);

void pixtro_init();
void pixtro_update();
void pixtro_render();

void open_file(int _file);
void save_file();
void reset_file();

void save_settings();
void reset_settings();

char char_from_file(int _index);
short short_from_file(int _index);
int int_from_file(int _index);
void char_to_file(int _index, char _value);
void short_to_file(int _index, short _value);
void int_to_file(int _index, int _value);

char char_from_settings(int _index);
short short_from_settings(int _index);
int int_from_settings(int _index);
void char_to_settings(int _index, char _value);
void short_to_settings(int _index, short _value);
void int_to_settings(int _index, int _value);

INLINE void key_mod(u32 key);
INLINE void key_mod2(u32 key);

INLINE void key_mod(u32 key)
{	__key_curr= key & KEY_MASK;	}

INLINE void key_mod2(u32 key)
{	__key_prev= key & KEY_MASK;	}

#endif
