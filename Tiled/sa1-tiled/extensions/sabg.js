const BGMode_Normal = 0;
const BGMode_Color256 = 1;
const BGMode_Scale = 2;

var sabgMapFormat = {
    name: "Sonic Advance Background",
    extension: "sabg",

	//Function for reading from a sabg file
	read: function(fileName) {
		var txtfile = new TextFile(fileName, TextFile.ReadOnly);
		var bginf = JSON.parse(txtfile.readAll());
		txtfile.close();

		var tilemap = new TileMap();
		tilemap.setTileSize(8, 8);
		tilemap.setSize(bginf.Width, bginf.Height);

		var palette = new Array(256);
		readPalette(bginf, palette);

		var file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(bginf.Tiles)), BinaryFile.ReadOnly);
		var tiles = new Uint8Array(file.readAll());
		file.close();

		if (bginf.Mode > BGMode_Normal) {
			for (var i = 16; i < 256; i += 16)
				palette[i] |= 0xFF000000;
			var tileset = new Tileset("Tiles");
			tileset.setTileSize(8, 8);
			var toff = 0;
			while (toff < tiles.length) {
				var pix = new Uint8Array(8 * 8);
				for (var y = 0; y < 8; ++y) {
					for (var x = 0; x < 8; ++x) {
						pix[y * 8 + x] = tiles[toff++];
					}
				}
				var img = new Image(pix.buffer, 8, 8, Image.Format_Indexed8);
				img.setColorTable(palette);
				var tile = tileset.addTile();
				tile.setImage(img);
			}
			tilemap.addTileset(tileset);

			if (bginf.Mode == BGMode_Color256) {
				file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(bginf.Layout)), BinaryFile.ReadOnly);
				var data = new Uint16Array(file.readAll());
				file.close();

				var layer = new TileLayer("Background");
				layer.width = bginf.Width;
				layer.height = bginf.Height;
				var fgedit = layer.edit();
				var addr = 0;
				for (var y = 0; y < bginf.Height; ++y) {
					for (var x = 0; x < bginf.Width; ++x) {
						var tinf = data[addr++];
						var tid = tinf & 0x3FF;
						if (tid >= tileset.tileCount)
							tid = 0;
						fgedit.setTile(x, y, tileset.tile(tid), (tinf & 0xC00) >> 10);
					}
				}
				fgedit.apply();
				tilemap.addLayer(layer);
			}
			else {
				file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(bginf.Layout)), BinaryFile.ReadOnly);
				var data = new Uint8Array(file.readAll());
				file.close();

				var layer = new TileLayer("Background");
				layer.width = bginf.Width;
				layer.height = bginf.Height;
				var fgedit = layer.edit();
				var addr = 0;
				for (var y = 0; y < bginf.Height; ++y) {
					for (var x = 0; x < bginf.Width; ++x) {
						var tid = data[addr++];
						if (tid >= tileset.tileCount)
							tid = 0;
						fgedit.setTile(x, y, tileset.tile(tid));
					}
				}
				fgedit.apply();
				tilemap.addLayer(layer);
			}
		}
		else {
			var tiles8 = new Array();
			var toff = 0;
			while (toff < tiles.length) {
				var pix = new Uint8Array(8 * 8);
				for (var y = 0; y < 8; ++y) {
					for (var x = 0; x < 8; x += 2) {
						var p = tiles[toff++];
						pix[y * 8 + x] = p & 0xF;
						pix[y * 8 + x + 1] = p >> 4;
					}
				}
				tiles8.push(pix.buffer);
			}


			var tilesets = new Array(16);
			for (var pn = 0; pn < 16; ++pn) {
				var ts = new Tileset("Tiles " + pn);
				ts.setTileSize(8, 8);
				ts.setProperty("Palette", pn);
				for (var ti = 0; ti < tiles8.length; ++ti) {
					var img = new Image(tiles8[ti], 8, 8, Image.Format_Indexed8);
					img.setColorTable(palette.slice(pn * 16, pn * 16 + 16));
					var tile = ts.addTile();
					tile.setImage(img);
				}
				tilesets[pn] = ts;
				tilemap.addTileset(ts);
			}

			file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(bginf.Layout)), BinaryFile.ReadOnly);
			var data = new Uint16Array(file.readAll());
			file.close();

			var layer = new TileLayer("Background");
			layer.width = bginf.Width;
			layer.height = bginf.Height;
			var fgedit = layer.edit();
			var addr = 0;
			for (var y = 0; y < bginf.Height; ++y) {
				for (var x = 0; x < bginf.Width; ++x) {
					var tinf = data[addr++];
					var tid = tinf & 0x3FF;
					if (tid >= tiles8.length)
						tid = 0;
					fgedit.setTile(x, y, tilesets[(tinf & 0xF000) >> 12].tile(tid), (tinf & 0xC00) >> 10);
				}
			}
			fgedit.apply();
			tilemap.addLayer(layer);
		}

		return tilemap;
	},


	write: function(map, fileName) {
		var txtfile = new TextFile(fileName, TextFile.ReadOnly);
		var bginf = JSON.parse(txtfile.readAll());
		txtfile.close();

		for (var lid = 0; lid < map.layerCount; ++lid) {
			var layer = map.layerAt(lid);
			switch (layer.name)
			{
				case "Background":
					if (layer.isTileLayer) {
						bginf.Width = layer.width;
						bginf.Height = layer.height;
						var data = null;
						if (bginf.Mode == BGMode_Scale) {
							data = new Uint8Array(layer.width * layer.height);
							for (var y = 0; y < layer.height; ++y) {
								for (var x = 0; x < layer.width; ++x) {
									var tile = layer.tileAt(x, y);
									if (tile != null)
										data[y * layer.width + x] = tile.id;
								}
							}
						}
						else {
							data = new Uint16Array(layer.width * layer.height);
							for (var y = 0; y < layer.height; ++y) {
								for (var x = 0; x < layer.width; ++x) {
									var tile = layer.tileAt(x, y);
									if (tile != null) {
										var tinf = tile.id;
										tinf |= (layer.flagsAt(x, y) & 3) << 10;
										if (bginf.Mode != BGMode_Color256)
											tinf |= (tile.tileset.property("Palette") ?? 0) << 12;
										data[y * layer.width + x] = tinf;
									}
								}
							}
						}
						var file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(bginf.Layout)), BinaryFile.WriteOnly);
						file.write(data.buffer);
						file.commit();

						txtfile = new TextFile(fileName, TextFile.WriteOnly);
						txtfile.write(JSON.stringify(bginf, null, 2));
						txtfile.commit();
					}
					break;
			}
		}
	}

}

tiled.registerMapFormat("sabg", sabgMapFormat);
