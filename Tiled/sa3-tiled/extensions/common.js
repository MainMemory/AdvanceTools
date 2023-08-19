var projpath = FileInfo.cleanPath(FileInfo.joinPaths(FileInfo.path(FileInfo.fromNativeSeparators(__filename)), "../.."));

var getTilemapImage = function(data, cnkaddr, width, height, tiles, palette)
{
	var pxw = width * 8;
	var pxh = height * 8;
	var pix = new Uint8Array(pxw * pxh);
	for (var cy = 0; cy < height; ++cy) {
		for (var cx = 0; cx < width; ++cx) {
			var tinf = data[cnkaddr++];
			var pal = (tinf & 0xF000) >> 8;
			var toff = (tinf & 0x3FF) * 0x20;
			switch (tinf & 0xC00)
			{
				case 0:
					for (var y = 0; y < 8; ++y) {
						for (var x = 0; x < 8; x += 2) {
							var p = tiles[toff++];
							pix[((cy * 8) + y) * pxw + (cx * 8) + x] = pal | (p & 0xF);
							pix[((cy * 8) + y) * pxw + (cx * 8) + x + 1] = pal | (p >> 4);
						}
					}
					break;
				case 0x400:
					for (var y = 0; y < 8; ++y) {
						for (var x = 6; x >= 0; x -= 2) {
							var p = tiles[toff++];
							pix[((cy * 8) + y) * pxw + (cx * 8) + x + 1] = pal | (p & 0xF);
							pix[((cy * 8) + y) * pxw + (cx * 8) + x] = pal | (p >> 4);
						}
					}
					break;
				case 0x800:
					for (var y = 7; y >= 0; --y) {
						for (var x = 0; x < 8; x += 2) {
							var p = tiles[toff++];
							pix[((cy * 8) + y) * pxw + (cx * 8) + x] = pal | (p & 0xF);
							pix[((cy * 8) + y) * pxw + (cx * 8) + x + 1] = pal | (p >> 4);
						}
					}
					break;
				case 0xC00:
					for (var y = 7; y >= 0; --y) {
						for (var x = 6; x >= 0; x -= 2) {
							var p = tiles[toff++];
							pix[((cy * 8) + y) * pxw + (cx * 8) + x + 1] = pal | (p & 0xF);
							pix[((cy * 8) + y) * pxw + (cx * 8) + x] = pal | (p >> 4);
						}
					}
					break;
			}
		}
	}
	var img = new Image(pix.buffer, pxw, pxh, Image.Format_Indexed8);
	img.setColorTable(palette);
	return img;
}

var readPalette = function(info, palette)
{
	if (info.Palette == null)
		return;
	var file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(info.Palette)), BinaryFile.ReadOnly);
	var data = new Uint16Array(file.readAll());
	file.close();

	for (var pi = 0; pi < data.length; ++pi) {
		var c = data[pi];
		var tmp = c & 0x1F;
		var r = (tmp >> 2) | (tmp << 3);
		tmp = (c >> 5) & 0x1F;
		var g = (tmp >> 2) | (tmp << 3);
		tmp = (c >> 10) & 0x1F;
		var b = (tmp >> 2) | (tmp << 3);
		c = (r << 16) | (g << 8) | b;
		if ((pi + info.PalDest) & 0xF)
			c |= 0xFF000000;
		palette[pi + info.PalDest] = c;
	}
}

var readChunks = function(name, info, palette)
{
	var tileset = new Tileset(name);
	tileset.setTileSize(info.ChunkWidth * 8, info.ChunkHeight * 8);

	var file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(info.Tiles)), BinaryFile.ReadOnly);
	var tiles = new Uint8Array(file.readAll());
	file.close();

	file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(info.Chunks)), BinaryFile.ReadOnly);
	var data = new Uint16Array(file.readAll());
	file.close();

	for (var cnkaddr = 0; cnkaddr < data.length; cnkaddr += info.ChunkWidth * info.ChunkHeight) {
		var tile = tileset.addTile();
		tile.setImage(getTilemapImage(
			data,
			cnkaddr,
			info.ChunkWidth,
			info.ChunkHeight,
			tiles,
			palette));
	}

	return tileset;
}

var readForegroundLayer = function(name, layinf, tileset)
{
	var layer = new TileLayer(name);
	layer.width = layinf.Width;
	layer.height = layinf.Height;

	var file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(layinf.Layout)), BinaryFile.ReadOnly);
	var data = new Uint16Array(file.readAll());
	file.close();

	var layaddr = 0;
	var fgedit = layer.edit();
	for (var y = 0; y < layinf.Height; ++y) {
		for (var x = 0; x < layinf.Width; ++x) {
			fgedit.setTile(x, y, tileset.tile(data[layaddr++]));
		}
	}
	fgedit.apply();
	return layer;
}

var readBackgroundLayer = function(name, info, palette)
{
	var layer = new ImageLayer(name);

	var file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(info.Tiles)), BinaryFile.ReadOnly);
	var tiles = new Uint8Array(file.readAll());
	file.close();

	file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(info.Layout)), BinaryFile.ReadOnly);
	var data = new Uint16Array(file.readAll());
	file.close();

	layer.image = getTilemapImage(
		data,
		0,
		info.Width,
		info.Height,
		tiles,
		palette);

	return layer;
}

var readObjects = function(filename, layer, className, hasType, dataCount)
{
	var file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(filename)), BinaryFile.ReadOnly);
	var data = new DataView(file.readAll());
	file.close();

	var width = data.getUint32(4, true);
	var height = data.getUint32(8, true);
	for (var ry = 0; ry < height; ++ry) {
		for (var rx = 0; rx < width; ++rx) {
			var off = data.getUint32(0xC + ((ry * width) + rx) * 4, true);
			if (off != 0) {
				off += 4;
				while (data.getUint8(off) != 0xFF) {
					var obj = new MapObject();
					obj.className = className;
					obj.shape = MapObject.Point;
					obj.x = data.getUint8(off++) * 8 + rx * 256;
					obj.y = data.getUint8(off++) * 8 + ry * 256;
					if (hasType)
						obj.setProperty("Type", data.getUint8(off++));
					for (var i = 0; i < dataCount; ++i)
						obj.setProperty("Data " + (i + 1), data.getUint8(off++));
					layer.addObject(obj);
				}
			}
		}
	}
}

var readInteractables = function(filename, layer)
{
	readObjects(filename, layer, "Interactable", true, 5);
}

var readItems = function(filename, layer)
{
	readObjects(filename, layer, "Item", true, 0);
}

var readEnemies = function(filename, layer)
{
	readObjects(filename, layer, "Enemy", true, 5);
}

var readRings = function(filename, layer)
{
	readObjects(filename, layer, "Ring", false, 0);
}

var writeForegroundLayer = function(filename, layer)
{
	var txtfile = new TextFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(filename)), TextFile.ReadOnly);
	var info = JSON.parse(txtfile.readAll());
	txtfile.close();

	if (info.Layout != null) {
		var data = new Uint16Array(layer.width * layer.height);
		var layaddr = 0;
		for (var y = 0; y < layer.height; ++y) {
			for (var x = 0; x < layer.width; ++x) {
				var tile = layer.tileAt(x, y);
				var tid = 0;
				if (tile != null)
					tid = tile.id;
				data[layaddr++] = tid;
			}
		}
		var file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(info.Layout)), BinaryFile.WriteOnly);
		file.write(data.buffer);
		file.commit();

		info.Width = layer.width;
		info.Height = layer.height;
		txtfile = new TextFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(filename)), TextFile.WriteOnly);
		txtfile.write(JSON.stringify(info, null, 2));
		txtfile.commit();
	}
}

var sortObjects = function(objects, width, height, hasType)
{
	var xrgns = (width + 255) >> 8;
	var yrgns = (height + 255) >> 8;
	var result = new Array(xrgns);
	for (var x = 0; x < xrgns; ++x) {
		result[x] = new Array(yrgns);
		for (var y = 0; y < yrgns; ++y)
			result[x][y] = new Array();
	}
	for (var i = 0; i < objects.length; ++i) {
		var obj = objects[i];
		if (obj.x >= 0 && obj.x < width && obj.y >= 0 && obj.y < height) {
			var rx = obj.x >> 8;
			var ry = obj.y >> 8;
			if (!hasType) {
				if (rx > 0 && Math.round((obj.x & 0xFF) / 8) == 0)
					--rx;
				if (ry > 0 && Math.round((obj.y & 0xFF) / 8) <= 1)
					--ry;
			}
			result[rx][ry].push(obj);
		}
	}
	for (var x = 0; x < xrgns; ++x)
		for (var y = 0; y < yrgns; ++y)
			result[x][y].sort((a, b) => {
				var res = a.y - b.y;
				if (res == 0)
					res = a.x - b.x;
				return res;
			});
	return {
		width: xrgns,
		height: yrgns,
		regions: result
	};
}

var writeObjects = function(filename, objects, width, height, hasType, dataCount)
{
	var objsize = 2;
	if (hasType)
		++objsize;
	objsize += dataCount;
	var regions = sortObjects(objects, width, height, hasType);
	var file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(filename)), BinaryFile.WriteOnly);
	file.seek(4);
	var data = new Uint32Array(2);
	data[0] = regions.width;
	data[1] = regions.height;
	file.write(data.buffer);
	file.write(new ArrayBuffer(regions.width * regions.height * 4));
	var ptroff = 0xC;
	var objoff = regions.width * regions.height * 4 + 0xC;
	for (var y = 0; y < regions.height; ++y) {
		for (var x = 0; x < regions.width; ++x) {
			var list = regions.regions[x][y];
			if (list.length > 0) {
				file.seek(ptroff);
				data = new Uint32Array(1);
				data[0] = objoff - 4;
				file.write(data.buffer);
				file.seek(objoff);
				data = new Uint8Array(objsize);
				for (var oi = 0; oi < list.length; ++oi) {
					var obj = list[oi];
					data[0] = Math.round((obj.x - x * 256) / 8);
					data[1] = Math.round((obj.y - y * 256) / 8);
					var di = 2;
					if (hasType)
						data[di++] = obj.resolvedProperty("Type");
					for (var dn = 0; dn < dataCount; ++dn)
						data[di++] = obj.resolvedProperty("Data " + (dn + 1));
					file.write(data.buffer);
				}
				data = new Uint8Array(1);
				data[0] = 0xFF;
				file.write(data.buffer);
				objoff = file.pos;
			}
			ptroff += 4;
		}
	}
	file.seek(0);
	data = new Uint32Array(1);
	data[0] = file.size << 8;
	file.write(data.buffer);
	file.commit();
}

var writeInteractables = function(filename, objects, width, height)
{
	writeObjects(filename, objects, width, height, true, 5);
}

var writeItems = function(filename, objects, width, height)
{
	writeObjects(filename, objects, width, height, true, 0);
}

var writeEnemies = function(filename, objects, width, height)
{
	writeObjects(filename, objects, width, height, true, 5);
}

var writeRings = function(filename, objects, width, height)
{
	writeObjects(filename, objects, width, height, false, 0);
}
