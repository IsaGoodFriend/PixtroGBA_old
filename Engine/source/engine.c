#include "engine.h"
#include <tonc.h>

#include "pixtro_basic.h"

#include "levels.h"
#include "load_data.h"

void update();
void render();

void on_update() {
	cam_x = FIXED2INT(entities[0].x);
	cam_y = FIXED2INT(entities[0].y);
}

void init() {
	
	LOAD_TILESET(test);
	
	load_bg_pal(PAL_test, 0);
	load_obj_pal(PAL_test, 0);
	
	load_sprite(&SPR_test_anim[0], 0, SPRITE16x16);
	
	set_layer_priority(1, 1);
	set_layer_priority(2, 2);
	set_layer_priority(3, 3);
	
	LOAD_BG(sample_ase, 1);
	
	set_foreground_count(1);
	finalize_layers();
	
	load_collision((unsigned char*)LVL_test);
	load_midground(0);
	load_entities();
	
	reset_cam();
	
	custom_update = &on_update;
	
	entities[0].ID |= ENT_ACTIVE_FLAG;
	entities[0].width = 12;
	entities[0].height = 16;
}

void init_settings() {
	
}