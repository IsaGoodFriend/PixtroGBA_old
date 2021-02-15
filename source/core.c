#include <tonc.h>
#include <string.h>

#include "sprites.h"

#include "core.h"
#include "graphics.h"
#include "math.h"

#define SAVE_INDEX (save_file_number * SAVEFILE_LEN) + SETTING_LEN

unsigned int GAME_freeze, GAME_life;
unsigned int fadeAmount, fading, loadIndex;

unsigned short* transition_style;
void (*hBlankInterrupt)(int);

unsigned int levelFlags, visualFlags;

char save_data[SAVEFILE_LEN - 1], settings_file[SETTING_LEN - 1];
int save_file_number;

Actor PHYS_actors[ACTOR_LIMIT];

void (*custom_update)(void);
void (*custom_render)(void);

void load_settings();
void interrupt();

void pixtro_init() {
	
	REG_DISPCNT = DCNT_BG0 | DCNT_OBJ | DCNT_OBJ_1D;
	
	irq_add( II_HBLANK, interrupt );
	
	init_drawing();
	load_settings();
	
	init();
}

void pixtro_update() {
	
	GAME_life++;
	
	if (custom_update)
		custom_update();
}

void pixtro_render() {
	
	if (custom_render)
		custom_render();
	
	end_drawing();
}


void interrupt(){
	
	return;
		
	int line = REG_VCOUNT;
	
	if (fadeAmount != TRANSITION_CAP && (line == 228 || line <= 160)) {
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
