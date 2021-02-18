#include <tonc.h>
#include <string.h>

#include "core.h"

#include "sprites.h"
#include "graphics.h"
#include "load_data.h"
#include "math.h"

#define SAVE_INDEX (save_file_number * SAVEFILE_LEN) + SETTING_LEN

// Layers
typedef struct Layer{
	int pos[8]; // The offsets of the lerp, excluding camera
	int lerp[8]; // combines both x and y, ranging from 0 - 0x100.
	
	unsigned int meta;
	unsigned int tile_meta; // the size and offset of the tiles used.  8 bits for offset, 8 bits for size
	unsigned int *tile_ptr;
	unsigned short *map_ptr;
	
} Layer;

#define LAYER_SIZE(n)			((layers[n].meta & 0x3) << 1)
#define LAYER_BLOCKSIZE(n)		((layers[n].meta & 0x3) >> 0)

#define TILESET_SIZE(n)			(layers[n].tile_meta & 0xFF)
#define TILESET_OFFSET(n)		((layers[n].tile_meta & 0xFF00) >> 8)
#define TILESET_SET(n, o, s)	layers[n].tile_meta = ((o & 0xFF) << 8) | (s & 0xFF) | TILES_CHANGED

#define TILES_CHANGED			0x10000
#define MAPPING_CHANGED			0x20000
#define LAYER_VIS_UPDATE		0x30000

#define FG_TILESET		0
#define BG_TILESET		1

int layer_count, layer_line[7], layer_index;
int bg_tile_allowance;

Layer layers[4];
int foreground_count;

// Entities
unsigned int max_entities;

int (*entity_inits[32])(unsigned int* actor_index, unsigned char* data);
Entity entities[ENTITY_LIMIT];
void (*entity_update[32])(int index);
void (*entity_render[32])(int index);

// Level data
char level_meta[128];

// Engine stuff
unsigned int GAME_freeze, GAME_life;
unsigned int fadeAmount, fading, loadIndex;

unsigned short* transition_style;
void (*hBlankInterrupt)(int);

unsigned int levelFlags, visualFlags;

char save_data[SAVEFILE_LEN - 1], settings_file[SETTING_LEN - 1];
int save_file_number;

int camX, camY, prevCamX, prevCamY;

void (*custom_update)(void);
void (*custom_render)(void);

void load_settings();
void interrupt();

void pixtro_init() {
	
	REG_DISPCNT = DCNT_BG0 | DCNT_BG1 | DCNT_BG2 | DCNT_BG3 | DCNT_OBJ | DCNT_OBJ_1D;
	
	set_layer_priority(1, 1);
	set_layer_priority(2, 2);
	set_layer_priority(3, 3);
	
	set_foreground_count(0);
	finalize_layers();
	
	irq_add( II_HBLANK, interrupt );
	
	init_drawing();
	load_settings();
	
	init();
}

void pixtro_update() {
	
	GAME_life++;
	
	int i;
	
	for (i = 0; i < max_entities; ++i){
		if (!ENT_FLAG(ACTIVE, i) || !entity_update[ENT_TYPE(i)])
			continue;
		
		entity_update[ENT_TYPE(i)](i);
	}
	
	if (custom_update)
		custom_update();
}

void pixtro_render() {
	
	layer_index = 0;
	
	int i;
	
	for (i = 0; i < max_entities; ++i){
		if (!ENT_FLAG(VISIBLE, i) || !entity_render[ENT_TYPE(i)])
			continue;
		
		entity_render[ENT_TYPE(i)](i);
	}
	
	if (custom_render)
		custom_render();
	
	end_drawing();
	
	move_cam();
}

// Layer functions that don't need to be finalized
void set_layer_visible(int layer, bool visible) {
	layer = 1 << (layer + 8);
	
	REG_DISPCNT = (REG_DISPCNT & ~layer) | (layer * visible);
}
void set_layer_priority(int layer, int prio) {
	// If the layer already has that priority, don't change anything
	if ((REG_BGCNT[layer] & BG_PRIO_MASK) == prio)
		return;
	
	int i;
	
	// Find the layer that has the same priority
	for (i = 0; i < 4; ++i) {
		if (i == layer)
			continue;
		
		if ((REG_BGCNT[i] & BG_PRIO_MASK) == prio){
			REG_BGCNT[i] = (REG_BGCNT[i] & ~BG_PRIO_MASK) | (REG_BGCNT[layer] & BG_PRIO_MASK);
			break;
		}
	}
	
	REG_BGCNT[layer] = (REG_BGCNT[layer] & ~BG_PRIO_MASK) | prio;
}

// Layer functions that require finalization
void load_background(int index, unsigned int *tiles, unsigned int tile_len, unsigned short *mapping) {
	if (index < foreground_count)
		return;
	
	load_background_tiles(index, tiles, tile_len);
	
	layers[index].map_ptr = mapping;
	layers[index].tile_meta |= MAPPING_CHANGED;
}
void load_background_tiles(int index, unsigned int *tiles, unsigned int tile_len) {
	int i;
	
	if (layers[index].tile_ptr != tiles) {
		
		if (index == 0)
			TILESET_SET(index, 0, tile_len);
		else
			TILESET_SET(index, TILESET_SIZE(index - 1) + TILESET_OFFSET(index - 1), tile_len);
		
		layers[index].tile_ptr = tiles;
		
		for (int i = index + 1; i < 4; ++i){
			
			TILESET_SET(i, TILESET_SIZE(i - 1) + TILESET_OFFSET(i - 1), TILESET_SIZE(i));
			layers[i].tile_meta |= MAPPING_CHANGED;
		}
		
	}
}
void set_foreground_count(int count) {
	foreground_count = count & 0x3;
}
// Finalize Layers
void finalize_layers() {
	int i;
	int sbb = 32;
	
	for (i = 0; i < 4; ++i) {
		REG_BGCNT[i] &= BG_PRIO_MASK; // Keep original priority, remove all other values
		
		int size;
		
		if (i >= foreground_count) {
			sbb -= LAYER_SIZE(i) + 1;
			
			size = LAYER_SIZE(i) + 1;
			
			REG_BGCNT[i] |= LAYER_SIZE(i) | BG_SBB(sbb) | BG_CBB(BG_TILESET);
		}
		else {
			--sbb;
			
			size = 1;
			
			REG_BGCNT[i] |= BG_SBB(sbb) | BG_CBB(FG_TILESET);
		}
		
		if (layers[i].tile_meta & LAYER_VIS_UPDATE) {
			
			if (layers[i].tile_meta & TILES_CHANGED) {
				
				memcpy(&tile_mem[BG_TILESET][TILESET_OFFSET(i)], layers[i].tile_ptr, TILESET_SIZE(i) << 5);
				
			}
			if (layers[i].tile_meta & MAPPING_CHANGED) {
				int index = 32 * 32 * size;
				
				int offset = TILESET_OFFSET(i);
				unsigned short *block = &se_mem[sbb], *mapping = layers[i].map_ptr;
				
				while (index) {
					
					--index;
					
					block[index] = mapping[index] + offset;
					
				}
				
			}
			
			layers[i].tile_meta &= 0xFFFF;
		}
	}
	
	sbb -= 8;
	bg_tile_allowance = sbb << 5;
}

void interrupt(){
	
	int line = REG_VCOUNT;
	
	if (fadeAmount != TRANSITION_CAP && (line == 228 || line < 160)) {
		if (IS_FADING) {
			if (line >= 160) {
				REG_WIN0H = transition_style[(fadeAmount >> 1) * 160];
			}
			else{
				REG_WIN0H = transition_style[line + (fadeAmount >> 1) * 160];
			}
		}
		
		if (hBlankInterrupt)
			hBlankInterrupt(line);
	}
	
}

//Settings File
void load_settings() {
	int index;
	
	if (sram_mem[0] == 0xFF) {
		
		sram_mem[0] = 0;
		
		for (index = 0; index < SETTING_LEN - 1; ++index) {
			settings_file[index] = 0;
		}
		
		init_settings();
		save_settings();
	}
	else {
		for (index = 0; index < SETTING_LEN - 1; ++index) {
			settings_file[index] = sram_mem[index + 1];
		}
	}
}
void save_settings() {
	int i;
	
	for (i = 0; i < SETTING_LEN - 1; ++i) {
		sram_mem[i + 1] = settings_file[i];
	}
}
void reset_settings() {
	int index;
	
	for (index = 0; index < SETTING_LEN; ++index) {
		sram_mem[index] = 0;
	}
}

// Save Files
void reset_file(){
	int index;
	int index2 = SAVE_INDEX;
	
	for (index = 0; index < SAVEFILE_LEN; ++index)
	{
		sram_mem[index + index2] = 0xFF;
	}
}
void save_file(){
	
	int index;
	int index2 = SAVE_INDEX;
	
	for (index = 0; index < SAVEFILE_LEN - 1; ++index)
	{
		sram_mem[index + index2 + 1] = save_data[index];
	}
}
void load_file(){
	
	int index = 0;
	int index2 = SAVE_INDEX;

	if (sram_mem[index2] == 0xFF) {
		for (index = 0; index < SAVEFILE_LEN - 1; ++index)
		{
			save_data[index] = sram_mem[index + index2 + 1];
		}
	}
	else {
		sram_mem[index2] = 0;
		 
		for (index = 0; index < SAVEFILE_LEN - 1; ++index) {
			save_data[index] = 0;
		}
		save_file();
	}
}
void open_file(int _file) {
	save_file_number = _file;
	load_file();
}

// Get and set from Save file
char char_from_file(int _index) {
	return save_data[_index];
}
short short_from_file(int _index) {
	return (save_data[_index]) + (save_data[_index + 1] << 8);
}
int int_from_file(int _index) {
	return (save_data[_index]) + (save_data[_index + 1] << 8) + (save_data[_index + 2] << 16) + (save_data[_index + 3] << 24);
}
void char_to_file(int _index, char _value) {
	save_data[_index] = _value;
}
void short_to_file(int _index, short _value) {
	int i;
	
	for (i = 0; i < 2; ++i) {
		save_data[_index + i] = _value & 0xFF;
		_value >>= 8;
	}
}
void int_to_file(int _index, int _value) {
	int i;
	
	for (i = 0; i < 4; ++i) {
		save_data[_index + i] = _value & 0xFF;
		_value >>= 8;
	}
}
// Get and set from settings
char char_from_settings(int _index) {
	return settings_file[_index];
}
short short_from_settings(int _index) {
	return (settings_file[_index]) + (settings_file[_index + 1] << 8);
}
int int_from_settings(int _index) {
	return (settings_file[_index]) + (settings_file[_index + 1] << 8) + (settings_file[_index + 2] << 16) + (settings_file[_index + 3] << 24);
}
void char_to_settings(int _index, char _value) {
	settings_file[_index] = _value;
}
void short_to_settings(int _index, short _value) {
	int i;
	
	for (i = 0; i < 2; ++i) {
		settings_file[_index + i] = _value & 0xFF;
		_value >>= 8;
	}
}
void int_to_settings(int _index, int _value) {
	int i;
	
	for (i = 0; i < 4; ++i) {
		settings_file[_index + i] = _value & 0xFF;
		_value >>= 8;
	}
}
