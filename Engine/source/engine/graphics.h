#pragma once

#include "sprites.h"
#include "math.h"

extern int drawing_flags;

// Drawing flags
#define DFLAG_CAM_FOLLOW		0x0001 // Do sprites use cam position data?

#define SET_DRAWING_FLAG(name)		drawing_flags |= DFLAG_##name;
#define CLEAR_DRAWING_FLAG(name)	drawing_flags &= ~DFLAG_##name;
#define DRAWING_FLAG_ENABLED(name)	(drawing_flags & DFLAG_##name);

// Sprite shapes
#define SPRITE8x8		0
#define SPRITE16x16		1
#define SPRITE32x32		2
#define SPRITE64x64		3

#define SPRITE16x8		4
#define SPRITE32x8		5
#define SPRITE32x16		6
#define SPRITE64x32		7

#define SPRITE8x16		8
#define SPRITE8x32		9
#define SPRITE16x32		10
#define SPRITE32x64		11

#define FLIP_NONE		0
#define FLIP_X			0x1000
#define FLIP_Y			0x2000
#define FLIP_XY			0x3000

#define LOAD_TILESET(name)		load_tileset((unsigned short*)TILE_##name, TILE_##name##_len)

extern int cam_x, cam_y;

void load_sprite(unsigned int *_sprite, int _index, int _shape);
void load_anim_sprite(unsigned int *sprites, int index, int shape, int frames, int speed);
void load_obj_pal(unsigned short *_pal, int _palIndex);
void load_bg_pal(unsigned short *_pal, int _palIndex);

void load_tileset(const unsigned short *_tiles, int _count);

void draw(int _x, int _y, int _sprite, int _flip, int _prio, int _pal);
void draw_affine(AffineMatrix _matrix, int _sprite, int _prio, int _pal);

void init_drawing();
void end_drawing();

void update_camera();
