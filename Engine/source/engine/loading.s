
.align	4
.data
adr_width:	.word
adr_height:	.word
	
.text
loading_width:	.word adr_width
loading_height:	.word adr_height

	.globl	load_header
	.code	32
@ void load_header(unsigned char* ptr)
@ r0 is always the level data pointer
load_header:
	
	ldr		r1, =level_toload
	ldr		r1, [r1]
	ldr		r3, loading_width
	ldrh	r2, [r0]
	strh	r2, [r1]
	str		r2, [r3]
	ldrh	r2, [r0, #2]
	strh	r2, [r1, #2]
	str		r2, [r3, #4]	@ load width and height into level storage
	
	add		r0,	#4
	add		r1,	#4
	
.ld_meta:
	ldr		r2,	[r0]		@ load meta index
	
	cmp		r2, #128
	bge		.ld_meta_e		@ skip if >= 128
	
	ldr		r3,	[r0, #1]	@ load meta value
	strb	r2, [r1]
	strb	r3,	[r1, #1]	@ save meta value
	add		r1, #2
	
	add		r0,	#2			@ move to next meta index value
	b		.ld_meta
.ld_meta_e:
	add		r0, #1
	
	mov		r2, #255
	lsl		r2, #8
	orr		r2, #255
	strh	r2, [r1]
	add		r1, #2
	
	@ --- end of loading ---
	ldr		r2, =level_toload
	str		r1, [r2]
	
	ldr		r1, =lvl_info
	str		r0, [r1]
	bx		lr
@ end load_header


	.align	4
	.globl	load_midground
	.code	32
@ void load_midground()
load_midground:
	
	push	{lr}

	ldr		r1,	=level_toload
	ldr		r1,	[r1]			@ r1 = level_toload

	ldr		r2, loading_width
	ldr		r0, [r2]
	ldr		r2, [r2, #4]
	mul		lr, r0, r2			@ lr = width * height;
	
	ldr		r0, =lvl_info
	ldr		r0, [r0]			@ r0 = lvl_info (get the pointer in lvl_info, not the pointer to lvl_info itself)
	
.ld_mid:
	ldrb	r3, [r0, #1]		@ r3 = value
	ldrb	r2, [r0, #2]
	lsl		r2, #8
	orr		r3, r2, r3

	ldrb	r2, [r0]			@ r2 = count
	add		r0, r0, #3

	@ while (r2 != 0) strh; r2--
.ld_mid_store:
	strh	r3, [r1]
	add		r1, #2

	sub		lr, #1
	cmp		lr, #0
	beq		.ld_mid_end			@ if --lr == 0, finish loading

	sub		r2, #1
	cmp		r2, #0
	bne		.ld_mid_store

	b		.ld_mid
	
.ld_mid_end:
	add		r1,	#2
	@ End of the function
	ldr		r2, =level_toload
	str		r1, [r2]
	
	ldr		r1, =lvl_info		@ store lvl_info value back to pointer
	str		r0, [r1]
	pop		{lr}
	bx		lr


@void fade_black(int factor) factor = 0-5
@ r0 = factor
	.align 4
	.globl fade_black
	.code 32
fade_black:
	
	cmp		r0, #0
	beq		.fade_allcolor
	cmp		r0, #5
	bge		.fade_allblack
	
	push	{lr}
	
	@ lr = mask
	mov		r2, #0
	mov		r1, r0
.fade_addloop:
	lsl		r2, #1
	add		r2, #1
	subs	r1, #1
	bne		.fade_addloop
	
	@ r1 = palette pointer
	mov		r1, #5
	lsl		r1, #24
	
	mov		r3, #5		@ lr <<= 5 - r0
	sub		r3, r3, r0
	lsl		r2, r3
	
	mov		lr, #1
	lsl		lr, #10
	orr		lr, r2
	lsl		lr, #5
	orr		lr, r2
	
	@ counter for 
	mov		r3, #512
	
	@ r2 = color_bank
	ldr		r2, =colorbank_bg
	
.fade_forloop:
	
	push	{r3}
	ldrh	r3, [r2]
	lsr		r3,	r0
	bic		r3, lr
	strh	r3, [r1]
	
	add		r1, #2
	add		r2, #2
	
	pop		{r3}
	subs	r3, #1
	bne		.fade_forloop
	
	pop		{lr}
	bx		lr


.fade_allblack:
	mov		r0, #5
	lsl		r0, #24

	mov		r1, #0
	
	mov		r2, #256
.fadeab_forloop:
	str		r1, [r0]
	add		r0, #4
	
	subs	r2, #1
	bne		.fadeab_forloop
	
	bx		lr
	
	
.fade_allcolor:
	@ r0 = palette pointer
	mov		r0, #5
	lsl		r0, #24

	@ r1 = color_bank
	ldr		r1, =colorbank_bg
	
	mov		r2, #256
.fadeac_forloop:
	ldr		r3, [r1]
	str		r3, [r0]
	add		r0, #4
	add		r1, #4
	
	subs	r2, #1
	bne		.fadeac_forloop
	
	bx		lr