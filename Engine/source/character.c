
void char_update(int index) {
	
	Entity *ent = &entities[index];
	ent->velY += 0x20;
	ent->velX = FIXED_APPROACH(ent->velX, (key_tri_horz() * 0x200), 0x40);
	
	
	if (key_pressed(KEY_A, 20)){
		ent->velY = -0x280;
		clear_buffer(KEY_A);
	}
	
	entity_physics(ent, 0x1, 0x1);
	
}
void char_render(int index) {
	
	Entity ent = entities[index];
	
	draw(ent.x - 0x200, ent.y, 0, 0, 0, 0);
}