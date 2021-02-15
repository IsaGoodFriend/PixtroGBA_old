#ifndef _PIX_GRAPHICS
#define _PIX_GRAPHICS

extern int drawing_flags;

// Drawing flags
#define DFLAG_CAM				0x0001 // Do sprites use cam position data?
#define DFLAG_LOAD_MAP			0x0002

#define SET_DRAWING_FLAG(f)		drawing_flags |= f;
#define CLEAR_DRAWING_FLAG(f)	drawing_flags &= ~f;

// Sprite shapes
#define SPRITE_8x8		0
#define SPRITE_16x16	1
#define SPRITE_32x32	2
#define SPRITE_64x64	3

#define SPRITE_16x8		4
#define SPRITE_32x8		5
#define SPRITE_32x16	6
#define SPRITE_64x32	7

#define SPRITE_8x16		8
#define SPRITE_8x32		9
#define SPRITE_16x32	10
#define SPRITE_32x64	11

#define FLIP_X			0x1000
#define FLIP_Y			0x2000

void load_sprite(unsigned short *_sprite, int _index, int _shape);
void load_obj_palette(unsigned short *_pal, int _palIndex);

void draw(int _x, int _y, int _sprite, int _flip, int _prio, int _pal);

void init_drawing();
void end_drawing();

void update_camera();

#endif
