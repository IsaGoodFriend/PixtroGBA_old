#include "engine.h"

#include "sprites.h"
#include "core.h"
#include "graphics.h"

void update();
void render();

void test_update(int index) {
	
}
void test_render(int index) {
	
}

void init() {
	custom_update = &update;
	custom_render = &render;
	
	load_sprite(SPR_test, 0, SPRITE_16x16);
	load_obj_pal(PAL_test, 0);
}

void init_settings() {
	
}

void update() {
	
}

void render() {
	
}