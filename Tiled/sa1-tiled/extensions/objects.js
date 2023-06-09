const PlayerAnim = {
	anim: 0,
	sub: 0,
	frame: 0,
	xflip: true,
	yflip: false,
	xmir: false,
	ymir: false
};
const RingAnim = {
	anim: 707,
	sub: 0,
	frame: 1,
	xflip: true,
	yflip: false,
	xmir: false,
	ymir: false
};
//\{ ([0-9]+), ([0-9]+), ([0-9]+), (false|true), (false|true), (false|true), (false|true) \}
const InteractableAnims = [
	/* 0x00 */
	{
		anim: 453,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 454,
		sub: 2,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 454,
		sub: 2,
		frame: 0,
		xflip: false,
		yflip: true,
		xmir: false,
		ymir: false
	},
	{
		anim: 454,
		sub: 3,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 454,
		sub: 3,
		frame: 0,
		xflip: false,
		yflip: true,
		xmir: false,
		ymir: false
	},
	{
		anim: 454,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 454,
		sub: 0,
		frame: 0,
		xflip: true,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 452,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 455,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 455,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 455,
		sub: 2,
		frame: 0,
		xflip: true,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 455,
		sub: 2,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 455,
		sub: 4,
		frame: 0,
		xflip: true,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 455,
		sub: 4,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 455,
		sub: 6,
		frame: 0,
		xflip: true,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 455,
		sub: 6,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},

	/* 0x10 */
	{
		anim: 458,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 463,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 463,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	}, // Falling
	{
		anim: 461,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 904,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 462,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 471,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},

	/* 0x20 */
	{
		anim: 471,
		sub: 1,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 471,
		sub: 2,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 717,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 469,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 473,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 468,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 468,
		sub: 0,
		frame: 0,
		xflip: true,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 472,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},

	/* 0x30 */
	{
		anim: 484,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 485,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 485,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 476,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 477,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 475,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 478,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 494,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 478,
		sub: 2,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 493,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 492,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 479,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	}, // data[0] -> Balloon Color
	{
		anim: 491,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 538,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 486,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 496,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},

	/* 0x40 */
	{
		anim: 497,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 488,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 488,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	}, /* Teleport (following two bytes = X/Y)*/
	{
		anim: 522,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 487,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 504,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 504,
		sub: 1,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 534,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 501,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},

	/* 0x50 */
	{
		anim: 498,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 540,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 510,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 547,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 463,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 542,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 558,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 545,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 523,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	}, /* Instant death*/
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 463,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 457,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 516,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 533,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 555,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	}, /* Destructable */

	/* 0x60 */
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	}, /* Force character to roll */
	{
		anim: 564,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 525,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 508,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 571,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 570,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 572,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 515,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 574,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	}, /* "Spin Levitating" in Casino Paradise*/
	{
		anim: 455,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	}, /* Hidden, until approached TODO: Maybe find a more accure animationIndex*/
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 589,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: true,
		xmir: false,
		ymir: false
	},
	{
		anim: 588,
		sub: 1,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},

	/* 0x70 */
	{
		anim: 592,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 559,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	},
	{
		anim: 767,
		sub: 0,
		frame: 0,
		xflip: false,
		yflip: false,
		xmir: false,
		ymir: false
	}
];
const ItemBoxAnim = 705;
const ItemAnim = 706;
const ItemSubs = [
	0,
	4,
	5,
	6,
	7,
	8,
	9,
	10
];
const EnemyAnims = [
	/* 0x00 */ 401,
	/* 0x01 */ 403,
	/* 0x02 */ 404,
	/* 0x03 */ 405,
	/* 0x04 */ 406,

	/* 0x05 */ 411,
	/* 0x06 */ 412,
	/* 0x07 */ 413,
	/* 0x08 */ 415,

	/* 0x09 */ 417,
	/* 0x0A */ 418,

	/* 0x0B */ 430,
	/* 0x0C */ 432,
	/* 0x0D */ 431,
	/* 0x0E */ 433,
	/* 0x0F */ 434,

	// Ice Mountain
	/* 0x10 */ 422,
	/* 0x11 */ 422, // sideways
	/* 0x12 */ 424,
	/* 0x13 */ 425,
	/* 0x14 */ 426,

	/* 0x15 */ 693,
	/* 0x16 */ 607,
	/* 0x17 */ 620,
	/* 0x18 */ 624,
	/* 0x19 */ 626,
	/* 0x1A */ 632,
	/* 0x1B */ 662,
	/* 0x1C */ 684,
	/* 0x1D */ 682,
	/* 0x1E */ 687,
	666
];

const AniCmd_DrawFrame = 0;
const AniCmd_GetTiles = -1;
const AniCmd_GetPalette = -2;
const AniCmd_JumpBack = -3;
const AniCmd_End = -4;
const AniCmd_PlaySfx = -5;
const AniCmd_6 = -6;
const AniCmd_TranslateSprite = -7;
const AniCmd_8 = -8;
const AniCmd_ChangeAnim = -9;
const AniCmd_10 = -10;
const AniCmd_11 = -11;
const AniCmd_12 = -12;
const ObjMode_Normal = 0;
const ObjMode_SemiTransparent = 1;
const ObjMode_Window = 2;
const ObjShape_Square = 0;
const ObjShape_Horizontal = 1;
const ObjShape_Vertical = 2;
const spriteSizes = [
	[
		{
			width: 1,
			height: 1
		},
		{
			width: 2,
			height: 2
		},
		{
			width: 4,
			height: 4
		},
		{
			width: 8,
			height: 8
		}
	],
	[
		{
			width: 2,
			height: 1
		},
		{
			width: 4,
			height: 1
		},
		{
			width: 4,
			height: 2
		},
		{
			width: 8,
			height: 4
		}
	],
	[
		{
			width: 1,
			height: 2
		},
		{
			width: 1,
			height: 4
		},
		{
			width: 2,
			height: 4
		},
		{
			width: 4,
			height: 8
		}
	]
];

var readAnimFile = function(filename)
{
	var file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(filename)), BinaryFile.ReadOnly);
	var data = new DataView(file.readAll());
	file.close();

	var result = new Array();
	var addr = 0;
	while (true) {
		var cmd = data.getInt32(addr, true);
		if (cmd >= 0) {
			result.push({
				type: AniCmd_DrawFrame,
				delay: cmd,
				frame: data.getInt32(addr + 4, true)
			});
			addr += 8;
		}
		else {
			switch (cmd)
			{
				case AniCmd_GetTiles:
					var ind = data.getUint32(addr + 4, true);
					result.push({
						type: cmd,
						color256: (ind & 0x80000000) == 0x80000000,
						tile: ind & 0x7FFFFFFF,
						count: data.getUint32(addr + 8, true)
					});
					addr += 12;
					break;
				case AniCmd_GetPalette:
					result.push({
						type: cmd,
						index: data.getInt32(addr + 4, true),
						count: data.getInt16(addr + 8, true),
						dest: data.getInt16(addr + 10, true)
					});
					addr += 12;
					break;
				case AniCmd_JumpBack:
					result.push({
						type: cmd,
						offset: data.getInt32(addr + 4, true)
					});
					return result;
				case AniCmd_End:
					result.push({
						type: cmd
					});
					return result;
				case AniCmd_PlaySfx:
					result.push({
						type: cmd,
						sfx: data.getUint16(addr + 4, true)
					});
					addr += 8;
					break;
				case AniCmd_6:
				case AniCmd_8:
					result.push({
						type: cmd,
						unk1: data.getInt32(addr + 4, true),
						unk2: data.getInt32(addr + 8, true)
					});
					addr += 12;
					break;
				case AniCmd_TranslateSprite:
					result.push({
						type: cmd,
						x: data.getInt16(addr + 4, true),
						y: data.getInt16(addr + 6, true)
					});
					addr += 8;
					break;
				case AniCmd_ChangeAnim:
					result.push({
						type: cmd,
						anim: data.getUint16(addr + 4, true),
						sub: data.getUint16(addr + 6, true)
					});
					return result;
				case AniCmd_10:
					result.push({
						type: cmd,
						unk1: data.getInt32(addr + 4, true),
						unk2: data.getInt32(addr + 8, true),
						unk3: data.getInt32(addr + 12, true)
					});
					addr += 16;
					break;
				case AniCmd_11:
				case AniCmd_12:
					result.push({
						type: cmd,
						unk1: data.getInt32(addr + 4, true)
					});
					addr += 8;
					break;
			}
		}
	}
}

var readMapFile = function(filename)
{
	var file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(filename)), BinaryFile.ReadOnly);
	var data = new DataView(file.readAll());
	file.close();

	var result = new Array();
	for (var addr = 0; addr < data.byteLength; addr += 12) {
		var flags = data.getUint8(addr);
		result.push({
			xflip: (flags & 1) == 1,
			yflip: (flags & 2) == 2,
			attr: data.getUint8(addr + 1),
			count: data.getUint16(addr + 2, true),
			width: data.getUint16(addr + 4, true),
			height: data.getUint16(addr + 6, true),
			x: data.getInt16(addr + 8, true),
			y: data.getInt16(addr + 10, true)
		});
	}
	return result;
}

var readAttrFile = function(filename)
{
	var file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(filename)), BinaryFile.ReadOnly);
	var data = new Uint16Array(file.readAll());
	file.close();

	var result = new Array();
	for (var addr = 0; addr < data.length; addr += 3) {
		var attrs = {};
		var val = data[addr];
		attrs.y = val & 0xFF;
		if (attrs.y >= 0x80)
			attrs.y -= 0x100;
		attrs.rotScl = (val & 0x100) == 0x100;
		if (attrs.rotScl)
			attrs.doubleSize = (val & 0x200) == 0x200;
		else
			attrs.disable = (val & 0x200) == 0x200;
		attrs.mode = (val >> 10) & 3;
		attrs.mosaic = (val & 0x1000) == 0x1000;
		attrs.color256 = (val & 0x2000) == 0x2000;
		attrs.shape = (val >> 14) & 3;
		val = data[addr + 1];
		attrs.x = val & 0x1FF;
		if (attrs.x >= 0x100)
			attrs.x -= 0x200;
		if (attrs.rotScl)
			attrs.rotSclParam = (val >> 9) & 0x1F;
		else {
			attrs.xflip = (val & 0x1000) == 0x1000;
			attrs.yflip = (val & 0x2000) == 0x2000;
		}
		attrs.size = (val >> 14) & 3;
		val = data[addr + 2];
		attrs.tile = val & 0x3FF;
		attrs.priority = (val >> 10) & 3;
		attrs.palette = (val >> 12) & 0xF;
		attrs.tileSize = spriteSizes[attrs.shape][attrs.size];
		result.push(attrs);
	}
	return result;
}

var getSprite = function(animNum, subNum, frameNum, palette, tiles16, tiles256)
{
	var proj = getProjectFile();
	var txtfile = new TextFile(FileInfo.joinPaths(projpath, proj.SpriteAnimations[animNum]), TextFile.ReadOnly);
	var anmvars = JSON.parse(txtfile.readAll());
	txtfile.close();
	var anim = readAnimFile(anmvars[subNum]);
	var maps = readMapFile(proj.SpriteMappings[animNum]);
	var attrs = readAttrFile(proj.SpriteAttributes[animNum]);
	var anipal = new Array(256);
	anipal.fill(0xFFFF00FF);
	var anitiles = new Array();
	for (var cmd of anim) {
		switch (cmd.type)
		{
			case AniCmd_GetTiles:
				if (cmd.color256)
					anitiles = tiles256.slice(cmd.tile * 0x40, (cmd.tile + cmd.count) * 0x40);
				else
					anitiles = tiles16.slice(cmd.tile * 0x20, (cmd.tile + cmd.count) * 0x20);
				break;
			case AniCmd_GetPalette:
				for (var i = 0; i < cmd.count; ++i)
					anipal[cmd.dest + i] = palette[(cmd.index * 16) + i];
				anipal[0] = 0;
				break;
			case AniCmd_DrawFrame:
				if (frameNum-- == 0) {
					var map = maps[cmd.frame];
					var xrad = Math.abs(map.x);
					var yrad = Math.abs(map.y);
					for (var sp = 0; sp < map.count; ++sp) {
						var attr = attrs[map.attr + sp];
						xrad = Math.max(xrad, Math.abs(-map.x + attr.x), Math.abs(-map.x + attr.x + (attr.tileSize.width * 8)));
						yrad = Math.max(yrad, Math.abs(-map.y + attr.y), Math.abs(-map.y + attr.y + (attr.tileSize.height * 8)));
					}
					if ((xrad & 1) == 1)
						++xrad;
					var width = xrad * 2;
					var pix = new Uint8Array(width * yrad * 2);
					for (var sp = 0; sp < map.count; ++sp) {
						var attr = attrs[map.attr + sp];
						var pxind = (yrad - map.y + attr.y) * width + (xrad - map.x + attr.x);
						var toff = attr.tile * 0x20;
						if (attr.color256)
							toff = attr.tile * 0x40;
						var pal = attr.palette << 4;
						for (var ty = 0; ty < attr.tileSize.height; ++ty) {
							for (var tx = 0; tx < attr.tileSize.width; ++tx) {
								var dst = pxind;
								if (attr.xflip)
									dst += (attr.tileSize.width - tx - 1) * 8;
								else
									dst += tx * 8;
								if (attr.yflip)
									dst += (attr.tileSize.height - ty - 1) * 8 * width;
								else
									dst += ty * 8 * width;
								if (!attr.color256) {
									if (attr.xflip) {
										if (attr.yflip) {
											for (var y = 7; y >= 0; --y) {
												for (var x = 6; x >= 0; x -= 2) {
													var p = anitiles[toff++];
													if ((p & 0xF) != 0)
														pix[dst + (y * width) + x + 1] = pal | (p & 0xF);
													if ((p & 0xF0) != 0)
														pix[dst + (y * width) + x] = pal | (p >> 4);
												}
											}
										}
										else {
											for (var y = 0; y < 8; ++y) {
												for (var x = 6; x >= 0; x -= 2) {
													var p = anitiles[toff++];
													if ((p & 0xF) != 0)
														pix[dst + (y * width) + x + 1] = pal | (p & 0xF);
													if ((p & 0xF0) != 0)
														pix[dst + (y * width) + x] = pal | (p >> 4);
												}
											}
										}
									}
									else {
										if (attr.yflip) {
											for (var y = 7; y >= 0; --y) {
												for (var x = 0; x < 8; x += 2) {
													var p = anitiles[toff++];
													if ((p & 0xF) != 0)
														pix[dst + (y * width) + x] = pal | (p & 0xF);
													if ((p & 0xF0) != 0)
														pix[dst + (y * width) + x + 1] = pal | (p >> 4);
												}
											}
										}
										else {
											for (var y = 0; y < 8; ++y) {
												for (var x = 0; x < 8; x += 2) {
													var p = anitiles[toff++];
													if ((p & 0xF) != 0)
														pix[dst + (y * width) + x] = pal | (p & 0xF);
													if ((p & 0xF0) != 0)
														pix[dst + (y * width) + x + 1] = pal | (p >> 4);
												}
											}
										}
									}
								}
								else { // color256
									if (attr.xflip) {
										if (attr.yflip) {
											for (var y = 7; y >= 0; --y) {
												for (var x = 7; x >= 0; --x) {
													pix[dst + (y * width) + x] = anitiles[toff++];
												}
											}
										}
										else {
											for (var y = 0; y < 8; ++y) {
												for (var x = 7; x >= 0; --x) {
													pix[dst + (y * width) + x] = anitiles[toff++];
												}
											}
										}
									}
									else {
										if (attr.yflip) {
											for (var y = 7; y >= 0; --y) {
												for (var x = 0; x < 8; ++x) {
													pix[dst + (y * width) + x] = anitiles[toff++];
												}
											}
										}
										else {
											for (var y = 0; y < 8; ++y) {
												for (var x = 0; x < 8; ++x) {
													pix[dst + (y * width) + x] = anitiles[toff++];
												}
											}
										}
									}
								}
							}
						}
					}
					var img = new Image(pix.buffer, width, yrad * 2, Image.Format_Indexed8);
					img.setColorTable(anipal);
					img = img.mirrored(map.xflip, map.yflip);
					return img;
				}
				break;
		}
	}
	return null;
}

var getSprites = function(anims, palette, tiles16, tiles256)
{
	var spritecache = new Array();
	var result = new Array();

	for (var anim of anims) {
		var image = null;
		if (anim.anim in spritecache) {
			if (anim.sub in spritecache[anim.anim]) {
				if (anim.frame in spritecache[anim.anim][anim.sub]) {
					image = spritecache[anim.anim][anim.sub][anim.frame];
				}
			}
			else {
				spritecache[anim.anim][anim.sub] = new Array();
			}
		}
		else {
			spritecache[anim.anim] = new Array();
			spritecache[anim.anim][anim.sub] = new Array();
		}
		if (image == null) {
			image = getSprite(anim.anim, anim.sub, anim.frame, palette, tiles16, tiles256);
			spritecache[anim.anim][anim.sub][anim.frame] = image;
		}
		result.push(image.mirrored(anim.xflip, anim.yflip));
	}
	return result;
}

var getInteractablesTileset = function(palette, tiles16, tiles256)
{
	var images = getSprites(InteractableAnims, palette, tiles16, tiles256);
	var tileset = new Tileset("Interactables");
	tileset.objectAlignment = Tileset.Center;
	for (var i = 0; i < images.length; ++i) {
		var tile = tileset.addTile();
		tile.setImage(images[i]);
		tile.className = "Interactable";
		tile.setProperty("Type", i);
	}
	return tileset;
}

var getEnemiesTileset = function(palette, tiles16, tiles256)
{
	var images = getSprites(EnemyAnims.map(a => ({ anim: a, sub: 0, frame: 0, xflip: false, yflip: false, xmir: false, ymir: false })), palette, tiles16, tiles256);
	var tileset = new Tileset("Enemies");
	tileset.objectAlignment = Tileset.Center;
	for (var i = 0; i < images.length; ++i) {
		var tile = tileset.addTile();
		tile.setImage(images[i]);
		tile.className = "Enemy";
		tile.setProperty("Type", i);
	}
	return tileset;
}

var getItemsTileset = function(palette, tiles16, tiles256)
{
	var images = getSprites(ItemSubs.map(a => ({ anim: ItemAnim, sub: a, frame: 0, xflip: false, yflip: false, xmir: false, ymir: false })), palette, tiles16, tiles256);
	var tileset = new Tileset("Items");
	tileset.objectAlignment = Tileset.Center;
	for (var i = 0; i < images.length; ++i) {
		var tile = tileset.addTile();
		tile.setImage(images[i]);
		tile.className = "Item";
		tile.setProperty("Type", i);
	}
	return tileset;
}

var getRingsTileset = function(palette, tiles16, tiles256)
{
	var images = getSprites([ RingAnim ], palette, tiles16, tiles256);
	var tileset = new Tileset("Rings");
	tileset.objectAlignment = Tileset.Center;
	for (var i = 0; i < images.length; ++i) {
		var tile = tileset.addTile();
		tile.setImage(images[i]);
		tile.className = "Ring";
	}
	return tileset;
}

var getPlayerTileset = function(palette, tiles16, tiles256)
{
	var images = getSprites([ PlayerAnim ], palette, tiles16, tiles256);
	var tileset = new Tileset("Player");
	tileset.objectAlignment = Tileset.Center;
	for (var i = 0; i < images.length; ++i) {
		var tile = tileset.addTile();
		tile.setImage(images[i]);
		tile.className = "Player";
	}
	return tileset;
}