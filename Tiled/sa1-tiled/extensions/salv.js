var salvMapFormat = {
    name: "Sonic Advance Level",
    extension: "salv",

	//Function for reading from a salv file
	read: function(fileName) {
		var txtfile = new TextFile(fileName, TextFile.ReadOnly);
		var stginf = JSON.parse(txtfile.readAll());
		txtfile.close();

		var w = 0;
		var h = 0;
		var chunkWidth = 0;
		var chunkHeight = 0;
		var palette = new Array(256);

		var foregroundHigh = null;
		if (stginf.ForegroundHigh != null) {
			txtfile = new TextFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(stginf.ForegroundHigh)), TextFile.ReadOnly);
			foregroundHigh = JSON.parse(txtfile.readAll());
			txtfile.close();
			w = foregroundHigh.Width;
			h = foregroundHigh.Height;
			chunkWidth = foregroundHigh.ChunkWidth;
			chunkHeight = foregroundHigh.ChunkHeight;
			readPalette(foregroundHigh, palette);
		}

		var foregroundLow = null;
		if (stginf.ForegroundLow != null) {
			txtfile = new TextFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(stginf.ForegroundLow)), TextFile.ReadOnly);
			foregroundLow = JSON.parse(txtfile.readAll());
			txtfile.close();
			if (w < foregroundLow.Width)
				w = foregroundLow.Width;
			if (h < foregroundLow.Height)
				h = foregroundLow.Height;
			if (chunkWidth < foregroundLow.ChunkWidth)
				chunkWidth = foregroundLow.ChunkWidth;
			if (chunkHeight < foregroundLow.ChunkHeight)
				chunkHeight = foregroundLow.ChunkHeight;
			readPalette(foregroundLow, palette);
		}

		var background = null;
		if (stginf.Background1 != null) {
			txtfile = new TextFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(stginf.Background1)), TextFile.ReadOnly);
			background = JSON.parse(txtfile.readAll());
			txtfile.close();
		}

		var tilemap = new TileMap();
		tilemap.setTileSize(chunkWidth * 8, chunkHeight * 8);
		tilemap.setSize(w, h);

		var separateChunks = false;
		if (foregroundHigh != null && foregroundLow != null)
			separateChunks = (foregroundHigh.Tiles != foregroundLow.Tiles) || (foregroundHigh.Chunks != foregroundLow.Chunks);

		var fghts = null;
		var fglts = null;

		if (!separateChunks) {
			fghts = readChunks("Chunks", foregroundHigh, palette);
			fglts = fghts;
			tilemap.addTileset(fghts);
		}
		else {
			if (foregroundHigh != null) {
				fghts = readChunks("FG High Chunks", foregroundHigh, palette);
				tilemap.addTileset(fghts);
			}
			if (foregroundLow != null) {
				fglts = readChunks("FG Low Chunks", foregroundLow, palette);
				tilemap.addTileset(fglts);
			}
		}

		if (background != null)
			tilemap.addLayer(readBackgroundLayer("Background", background, palette));
		if (foregroundLow != null)
			tilemap.addLayer(readForegroundLayer("Foreground Low", foregroundLow, fghts));
		if (foregroundHigh != null)
			tilemap.addLayer(readForegroundLayer("Foreground High", foregroundHigh, fglts));

		var layer = new ObjectGroup("Objects");

		var proj = getProjectFile();

		var palette = new Array();
		var file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(proj.SpritePalettes)), BinaryFile.ReadOnly);
		var data = new Uint16Array(file.readAll());
		file.close();
		for (var c of data)
			palette.push(colorGBAToRGB(c));

		var file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(proj.SpriteTiles16)), BinaryFile.ReadOnly);
		var tiles16 = new Uint8Array(file.readAll());
		file.close();

		var file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(proj.SpriteTiles256)), BinaryFile.ReadOnly);
		var tiles256 = new Uint8Array(file.readAll());
		file.close();

		if (stginf.PlayerStart != null) {
			var ts = getPlayerTileset(palette, tiles16, tiles256);
			tilemap.addTileset(ts);

			var file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(stginf.PlayerStart)), BinaryFile.ReadOnly);
			var data = new Uint16Array(file.readAll());
			file.close();

			var obj = new MapObject();
			obj.className = "Player";
			obj.tile = ts.tile(0);
			obj.size = obj.tile.size;
			obj.x = data[0];
			obj.y = data[1];
			layer.addObject(obj);
		}

		if (stginf.Interactables != null) {
			var ts = getInteractablesTileset(palette, tiles16, tiles256);
			tilemap.addTileset(ts);
			readInteractables(stginf.Interactables, layer, ts);
		}
		if (stginf.Items != null) {
			var ts = getItemsTileset(palette, tiles16, tiles256);
			tilemap.addTileset(ts);
			readItems(stginf.Items, layer, ts);
		}
		if (stginf.Enemies != null) {
			var ts = getEnemiesTileset(palette, tiles16, tiles256);
			tilemap.addTileset(ts);
			readEnemies(stginf.Enemies, layer, ts);
		}
		if (stginf.Rings != null) {
			var ts = getRingsTileset(palette, tiles16, tiles256);
			tilemap.addTileset(ts);
			readRings(stginf.Rings, layer, ts);
		}

		tilemap.addLayer(layer);

		return tilemap;
	},


	write: function(map, fileName) {
		var txtfile = new TextFile(fileName, TextFile.ReadOnly);
		var stginf = JSON.parse(txtfile.readAll());
		txtfile.close();

		for (var lid = 0; lid < map.layerCount; ++lid) {
			var layer = map.layerAt(lid);
			switch (layer.name)
			{
				case "Foreground High":
					if (layer.isTileLayer && stginf.ForegroundHigh != null)
						writeForegroundLayer(stginf.ForegroundHigh, layer);
					break;
				case "Foreground Low":
					if (layer.isTileLayer && stginf.ForegroundLow != null)
						writeForegroundLayer(stginf.ForegroundLow, layer);
					break;
				case "Objects":
					if (layer.isObjectLayer) {
						var interactables = new Array();
						var items = new Array();
						var enemies = new Array();
						var rings = new Array();
						var start = null;
						for (var oid = 0; oid < layer.objectCount; oid++) {
							var obj = layer.objectAt(oid);
							switch (obj.className)
							{
								case "Interactable":
									interactables.push(obj);
									break;
								case "Item":
									items.push(obj);
									break;
								case "Enemy":
									enemies.push(obj);
									break;
								case "Ring":
									rings.push(obj);
									break;
								case "Player":
									start = obj;
									break;
							}
						}
						if (stginf.Interactables != null)
							writeInteractables(stginf.Interactables, interactables, map.width * map.tileWidth, map.height * map.tileHeight);
						if (stginf.Items != null)
							writeItems(stginf.Items, items, map.width * map.tileWidth, map.height * map.tileHeight);
						if (stginf.Enemies != null)
							writeEnemies(stginf.Enemies, enemies, map.width * map.tileWidth, map.height * map.tileHeight);
						if (stginf.Rings != null)
							writeRings(stginf.Rings, rings, map.width * map.tileWidth, map.height * map.tileHeight);
						if (stginf.PlayerStart != null) {
							var data = new Uint16Array(2);
							if (start != null) {
								data[0] = start.x;
								data[1] = start.y;
							}
							var file = new BinaryFile(FileInfo.joinPaths(projpath, FileInfo.fromNativeSeparators(stginf.PlayerStart)), BinaryFile.WriteOnly);
							file.write(data.buffer);
							file.commit();
						}
					}
					break;
			}
		}
	}

}

tiled.registerMapFormat("salv", salvMapFormat);
