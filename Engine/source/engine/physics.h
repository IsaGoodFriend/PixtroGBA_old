#ifndef _PIX_PHYSICS
#define _PIX_PHYSICS

#include "core.h"


extern unsigned int entity_physics(Entity *ent, int hitMask, int detectMask);
//extern unsigned int collide_rect(int x, int y, int width, int height);
//extern unsigned int collide_entity(unsigned int ID);

int GetEntityX(int value, int camera);
int GetEntityY(int value, int camera);

#endif
