#pragma once

#define __DEBUG__

#include <tonc.h>
#include <string.h>

#include "engine.h"

// ---- Entities ----
typedef struct Entity
{
	int x, y, vel_x, vel_y;
	unsigned short width, height;
	unsigned int ID, collision_flags;
	
	unsigned int flags[5];
	
	
} ALIGN4 Entity;

#define ENT_TYPE(n)					(entities[n].ID & 0xFF)

#define ENT_FLAG(name, n)			((entities[n].ID & ENT_##name##_FLAG) >> ENT_##name##_SHIFT)

#define ENT_PERSISTENT_FLAG			0x00000100
#define ENT_PERSISTENT_SHIFT		8
#define ENT_ACTIVE_FLAG				0x00000200
#define ENT_ACTIVE_SHIFT			9
#define ENT_VISIBLE_FLAG			0x00000400
#define ENT_VISIBLE_SHIFT			10

extern unsigned int max_entities;

extern int (*entity_inits[32])(unsigned int* actor_index, unsigned char* data);
extern Entity entities[ENTITY_LIMIT];
extern void (*entity_update[32])(int index);
extern void (*entity_render[32])(int index);

// ---- LAYERS ----

typedef struct Layer{
	int pos[8]; // The offsets of the lerp, excluding camera
	int lerp[8]; // combines both x and y, ranging from 0 - 0x100.
	
	unsigned int meta;
	unsigned int tile_meta; // the size and offset of the tiles used.  8 bits for offset, 8 bits for size
	unsigned int *tile_ptr;
	unsigned short *map_ptr;
	
} Layer;

extern int layer_count, layer_line[7], layer_index;
extern int bg_tile_allowance;

extern Layer layers[4];
extern int foreground_count;

// Macro to help load backgrounds easier.  
#define LOAD_BG(bg, n) load_background(n, BGT_##bg, BGT_##bg##_len, BG_##bg, BG_##bg##_size)

#define FG_TILESET		0
#define BG_TILESET		1

// ---- ENGINE ----

//#define START_FADE()		0//GAME_fading = 1; GAME_fadeAmount = 0; GAME_loadIndex = 0
#define TRANSITION_CAP		16

extern unsigned int game_freeze, game_life;

extern unsigned short* transition_style;

extern unsigned int visualFlags;

// The size of blocks in a level.
#ifdef LARGE_TILES

#define BLOCK_SIZE			16
#define BLOCK_SHIFT			4

#else

#define BLOCK_SIZE			8
#define BLOCK_SHIFT			3

#endif



extern unsigned int levelFlags;
extern char gamestate, nextGamestate;

#define IS_FADING 0//(GAME_fading || GAME_fadeAmount)
extern int GAME_fadeAmount, GAME_fading, GAME_loadIndex;

extern void (*custom_update)(void);
extern void (*custom_render)(void);

void pixtro_init();
void pixtro_update();
void pixtro_render();

void set_layer_visible(int layer, bool vis);
void set_layer_priority(int layer, int prio);
void set_foreground_count(int count);
void load_background(int index, unsigned int *tiles, unsigned int tile_len, unsigned short *mapping, int size);
void finalize_layers();

void open_file(int file);
void save_file();
void reset_file();

void save_settings();
void reset_settings();

char char_from_file(int index);
short short_from_file(int index);
int int_from_file(int index);
void char_to_file(int index, char value);
void short_to_file(int index, short value);
void int_to_file(int index, int value);

char char_from_settings(int index);
short short_from_settings(int index);
int int_from_settings(int index);
void char_to_settings(int index, char value);
void short_to_settings(int index, short value);
void int_to_settings(int index, int value);

INLINE void key_mod(u32 key);
INLINE void key_mod2(u32 key);

INLINE void key_mod(u32 key)
{	__key_curr= key & KEY_MASK;	}

INLINE void key_mod2(u32 key)
{	__key_prev= key & KEY_MASK;	}

