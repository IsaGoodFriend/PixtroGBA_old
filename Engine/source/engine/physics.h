#ifndef _PIX_PHYSICS
#define _PIX_PHYSICS

#include "core.h"

extern unsigned char yShift;
extern unsigned int width, height;
extern unsigned char collisionData[4096];

extern unsigned int entity_physics(Entity *ent, int hitMask, int detectMask);
//extern unsigned int collide_rect(int x, int y, int width, int height);
//extern unsigned int collide_entity(unsigned int ID);

int GetEntityX(int value, int camera);
int GetEntityY(int value, int camera);

#endif
