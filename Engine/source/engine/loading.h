#pragma once

void load_header(unsigned char* ptr);
void load_midground();
// Set screen fade amount.  Factor should be between 0 and 5
void fade_black(unsigned int factor);