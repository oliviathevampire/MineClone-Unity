using System.Collections.Generic;
using UnityEngine;

public class TextureMapper {
	public Dictionary<Blocks.Block, TextureMap> map;

	public TextureMapper() {
		map = new Dictionary<Blocks.Block, TextureMap> {
		   {
			   Blocks.Grass, new TextureMap(
				   new TextureMap.Face(new Vector2Int(0, 1)),
				   new TextureMap.Face(new Vector2Int(0, 1)),
				   new TextureMap.Face(new Vector2Int(0, 1)),
				   new TextureMap.Face(new Vector2Int(0, 1)),
				   new TextureMap.Face(new Vector2Int(0, 0)),
				   new TextureMap.Face(new Vector2Int(0, 2))
			   )
		   }, {
			   Blocks.Podzol, new TextureMap(
				   new TextureMap.Face(new Vector2Int(0, 5)),
				   new TextureMap.Face(new Vector2Int(0, 5)),
				   new TextureMap.Face(new Vector2Int(0, 5)),
				   new TextureMap.Face(new Vector2Int(0, 5)),
				   new TextureMap.Face(new Vector2Int(0, 4)),
				   new TextureMap.Face(new Vector2Int(0, 2))
			   )
		   }, {
			   Blocks.Dirt, new TextureMap(
				   new TextureMap.Face(new Vector2Int(0, 2)),
				   new TextureMap.Face(new Vector2Int(0, 2)),
				   new TextureMap.Face(new Vector2Int(0, 2)),
				   new TextureMap.Face(new Vector2Int(0, 2)),
				   new TextureMap.Face(new Vector2Int(0, 2)),
				   new TextureMap.Face(new Vector2Int(0, 2))
			   )
		   }, {
			   Blocks.Sand, new TextureMap(
				   new TextureMap.Face(new Vector2Int(2, 5)),
				   new TextureMap.Face(new Vector2Int(2, 5)),
				   new TextureMap.Face(new Vector2Int(2, 5)),
				   new TextureMap.Face(new Vector2Int(2, 5)),
				   new TextureMap.Face(new Vector2Int(2, 5)),
				   new TextureMap.Face(new Vector2Int(2, 5))
			   )
		   }, {
			   Blocks.Sandstone, new TextureMap(
				   new TextureMap.Face(new Vector2Int(3, 6)),
				   new TextureMap.Face(new Vector2Int(3, 6)),
				   new TextureMap.Face(new Vector2Int(3, 6)),
				   new TextureMap.Face(new Vector2Int(3, 6)),
				   new TextureMap.Face(new Vector2Int(3, 5)),
				   new TextureMap.Face(new Vector2Int(3, 7))
			   )
		   }, {
			   Blocks.Stone, new TextureMap(
				   new TextureMap.Face(new Vector2Int(1, 0)),
				   new TextureMap.Face(new Vector2Int(1, 0)),
				   new TextureMap.Face(new Vector2Int(1, 0)),
				   new TextureMap.Face(new Vector2Int(1, 0)),
				   new TextureMap.Face(new Vector2Int(1, 0)),
				   new TextureMap.Face(new Vector2Int(1, 0))
			   )
		   }, {
			   Blocks.Deepslate, new TextureMap(
				   new TextureMap.Face(new Vector2Int(15, 2)),
				   new TextureMap.Face(new Vector2Int(15, 2)),
				   new TextureMap.Face(new Vector2Int(15, 2)),
				   new TextureMap.Face(new Vector2Int(15, 2)),
				   new TextureMap.Face(new Vector2Int(15, 1)),
				   new TextureMap.Face(new Vector2Int(15, 1))
			   )
		   }, {
			   Blocks.Bedrock, new TextureMap(
				   new TextureMap.Face(new Vector2Int(2, 0)),
				   new TextureMap.Face(new Vector2Int(2, 0)),
				   new TextureMap.Face(new Vector2Int(2, 0)),
				   new TextureMap.Face(new Vector2Int(2, 0)),
				   new TextureMap.Face(new Vector2Int(2, 0)),
				   new TextureMap.Face(new Vector2Int(2, 0))
			   )
		   }, {
			   Blocks.CoalOre, new TextureMap(
				   new TextureMap.Face(new Vector2Int(1, 1)),
				   new TextureMap.Face(new Vector2Int(1, 1)),
				   new TextureMap.Face(new Vector2Int(1, 1)),
				   new TextureMap.Face(new Vector2Int(1, 1)),
				   new TextureMap.Face(new Vector2Int(1, 1)),
				   new TextureMap.Face(new Vector2Int(1, 1))
			   )
		   }, {
			   Blocks.IronOre, new TextureMap(
				   new TextureMap.Face(new Vector2Int(1, 2)),
				   new TextureMap.Face(new Vector2Int(1, 2)),
				   new TextureMap.Face(new Vector2Int(1, 2)),
				   new TextureMap.Face(new Vector2Int(1, 2)),
				   new TextureMap.Face(new Vector2Int(1, 2)),
				   new TextureMap.Face(new Vector2Int(1, 2))
			   )
		   }, {
			   Blocks.GoldOre, new TextureMap(
				   new TextureMap.Face(new Vector2Int(1, 3)),
				   new TextureMap.Face(new Vector2Int(1, 3)),
				   new TextureMap.Face(new Vector2Int(1, 3)),
				   new TextureMap.Face(new Vector2Int(1, 3)),
				   new TextureMap.Face(new Vector2Int(1, 3)),
				   new TextureMap.Face(new Vector2Int(1, 3))
			   )
		   }, {
			   Blocks.DiamondOre, new TextureMap(
				   new TextureMap.Face(new Vector2Int(1, 4)),
				   new TextureMap.Face(new Vector2Int(1, 4)),
				   new TextureMap.Face(new Vector2Int(1, 4)),
				   new TextureMap.Face(new Vector2Int(1, 4)),
				   new TextureMap.Face(new Vector2Int(1, 4)),
				   new TextureMap.Face(new Vector2Int(1, 4))
			   )
		   }, {
			   Blocks.OakLog, new TextureMap(
				   new TextureMap.Face(new Vector2Int(0, 11)),
				   new TextureMap.Face(new Vector2Int(0, 11)),
				   new TextureMap.Face(new Vector2Int(0, 11)),
				   new TextureMap.Face(new Vector2Int(0, 11)),
				   new TextureMap.Face(new Vector2Int(0, 10)),
				   new TextureMap.Face(new Vector2Int(0, 10))
			   )
		   }, {
			   Blocks.BirchLog, new TextureMap(
				   new TextureMap.Face(new Vector2Int(1, 11)),
				   new TextureMap.Face(new Vector2Int(1, 11)),
				   new TextureMap.Face(new Vector2Int(1, 11)),
				   new TextureMap.Face(new Vector2Int(1, 11)),
				   new TextureMap.Face(new Vector2Int(1, 10)),
				   new TextureMap.Face(new Vector2Int(1, 10))
			   )
		   }, {
			   Blocks.DarkOakLog, new TextureMap(
				   new TextureMap.Face(new Vector2Int(2, 11)),
				   new TextureMap.Face(new Vector2Int(2, 11)),
				   new TextureMap.Face(new Vector2Int(2, 11)),
				   new TextureMap.Face(new Vector2Int(2, 11)),
				   new TextureMap.Face(new Vector2Int(2, 10)),
				   new TextureMap.Face(new Vector2Int(2, 10))
			   )
		   }, {
			   Blocks.SpruceLog, new TextureMap(
				   new TextureMap.Face(new Vector2Int(3, 11)),
				   new TextureMap.Face(new Vector2Int(3, 11)),
				   new TextureMap.Face(new Vector2Int(3, 11)),
				   new TextureMap.Face(new Vector2Int(3, 11)),
				   new TextureMap.Face(new Vector2Int(3, 10)),
				   new TextureMap.Face(new Vector2Int(3, 10))
			   )
		   }, {
			   Blocks.AcaciaLog, new TextureMap(
				   new TextureMap.Face(new Vector2Int(4, 11)),
				   new TextureMap.Face(new Vector2Int(4, 11)),
				   new TextureMap.Face(new Vector2Int(4, 11)),
				   new TextureMap.Face(new Vector2Int(4, 11)),
				   new TextureMap.Face(new Vector2Int(4, 10)),
				   new TextureMap.Face(new Vector2Int(4, 10))
			   )
		   }, {
			   Blocks.OakPlanks, new TextureMap(
				   new TextureMap.Face(new Vector2Int(0, 9)),
				   new TextureMap.Face(new Vector2Int(0, 9)),
				   new TextureMap.Face(new Vector2Int(0, 9)),
				   new TextureMap.Face(new Vector2Int(0, 9)),
				   new TextureMap.Face(new Vector2Int(0, 9)),
				   new TextureMap.Face(new Vector2Int(0, 9))
			   )
		   }, {
			   Blocks.BirchPlanks, new TextureMap(
				   new TextureMap.Face(new Vector2Int(1, 9)),
				   new TextureMap.Face(new Vector2Int(1, 9)),
				   new TextureMap.Face(new Vector2Int(1, 9)),
				   new TextureMap.Face(new Vector2Int(1, 9)),
				   new TextureMap.Face(new Vector2Int(1, 9)),
				   new TextureMap.Face(new Vector2Int(1, 9))
			   )
		   }, {
			   Blocks.DarkOakPlanks, new TextureMap(
				   new TextureMap.Face(new Vector2Int(2, 9)),
				   new TextureMap.Face(new Vector2Int(2, 9)),
				   new TextureMap.Face(new Vector2Int(2, 9)),
				   new TextureMap.Face(new Vector2Int(2, 9)),
				   new TextureMap.Face(new Vector2Int(2, 9)),
				   new TextureMap.Face(new Vector2Int(2, 9))
			   )
		   }, {
			   Blocks.SprucePlanks, new TextureMap(
				   new TextureMap.Face(new Vector2Int(3, 9)),
				   new TextureMap.Face(new Vector2Int(3, 9)),
				   new TextureMap.Face(new Vector2Int(3, 9)),
				   new TextureMap.Face(new Vector2Int(3, 9)),
				   new TextureMap.Face(new Vector2Int(3, 9)),
				   new TextureMap.Face(new Vector2Int(3, 9))
			   )
		   }, {
			   Blocks.AcaciaPlanks, new TextureMap(
				   new TextureMap.Face(new Vector2Int(4, 9)),
				   new TextureMap.Face(new Vector2Int(4, 9)),
				   new TextureMap.Face(new Vector2Int(4, 9)),
				   new TextureMap.Face(new Vector2Int(4, 9)),
				   new TextureMap.Face(new Vector2Int(4, 9)),
				   new TextureMap.Face(new Vector2Int(4, 9))
			   )
		   }, {
			   Blocks.OakLeaves, new TextureMap(
				   new TextureMap.Face(new Vector2Int(0, 12)),
				   new TextureMap.Face(new Vector2Int(0, 12)),
				   new TextureMap.Face(new Vector2Int(0, 12)),
				   new TextureMap.Face(new Vector2Int(0, 12)),
				   new TextureMap.Face(new Vector2Int(0, 12)),
				   new TextureMap.Face(new Vector2Int(0, 12))
			   )
		   }, {
			   Blocks.BirchLeaves, new TextureMap(
				   new TextureMap.Face(new Vector2Int(1, 12)),
				   new TextureMap.Face(new Vector2Int(1, 12)),
				   new TextureMap.Face(new Vector2Int(1, 12)),
				   new TextureMap.Face(new Vector2Int(1, 12)),
				   new TextureMap.Face(new Vector2Int(1, 12)),
				   new TextureMap.Face(new Vector2Int(1, 12))
			   )
		   }, {
			   Blocks.Glowstone, new TextureMap(
				   new TextureMap.Face(new Vector2Int(3, 0)),
				   new TextureMap.Face(new Vector2Int(3, 0)),
				   new TextureMap.Face(new Vector2Int(3, 0)),
				   new TextureMap.Face(new Vector2Int(3, 0)),
				   new TextureMap.Face(new Vector2Int(3, 0)),
				   new TextureMap.Face(new Vector2Int(3, 0))
			   )
		   }, {
			   Blocks.Andesite, new TextureMap(
				   new TextureMap.Face(new Vector2Int(2, 3)),
				   new TextureMap.Face(new Vector2Int(2, 3)),
				   new TextureMap.Face(new Vector2Int(2, 3)),
				   new TextureMap.Face(new Vector2Int(2, 3)),
				   new TextureMap.Face(new Vector2Int(2, 3)),
				   new TextureMap.Face(new Vector2Int(2, 3))
			   )
		   }, {
			   Blocks.Diorite, new TextureMap(
				   new TextureMap.Face(new Vector2Int(2, 1)),
				   new TextureMap.Face(new Vector2Int(2, 1)),
				   new TextureMap.Face(new Vector2Int(2, 1)),
				   new TextureMap.Face(new Vector2Int(2, 1)),
				   new TextureMap.Face(new Vector2Int(2, 1)),
				   new TextureMap.Face(new Vector2Int(2, 1))
			   )
		   }, {
			   Blocks.Granite, new TextureMap(
				   new TextureMap.Face(new Vector2Int(2, 2)),
				   new TextureMap.Face(new Vector2Int(2, 2)),
				   new TextureMap.Face(new Vector2Int(2, 2)),
				   new TextureMap.Face(new Vector2Int(2, 2)),
				   new TextureMap.Face(new Vector2Int(2, 2)),
				   new TextureMap.Face(new Vector2Int(2, 2))
			   )
		   }, {
			   Blocks.Calcite, new TextureMap(
				   new TextureMap.Face(new Vector2Int(8, 0)),
				   new TextureMap.Face(new Vector2Int(8, 0)),
				   new TextureMap.Face(new Vector2Int(8, 0)),
				   new TextureMap.Face(new Vector2Int(8, 0)),
				   new TextureMap.Face(new Vector2Int(8, 0)),
				   new TextureMap.Face(new Vector2Int(8, 0))
			   )
		   }, {
			   Blocks.Tuff, new TextureMap(
				   new TextureMap.Face(new Vector2Int(5, 0)),
				   new TextureMap.Face(new Vector2Int(5, 0)),
				   new TextureMap.Face(new Vector2Int(5, 0)),
				   new TextureMap.Face(new Vector2Int(5, 0)),
				   new TextureMap.Face(new Vector2Int(5, 0)),
				   new TextureMap.Face(new Vector2Int(5, 0))
			   )
		   }, {
			   Blocks.Terracotta, new TextureMap(
				   new TextureMap.Face(new Vector2Int(6, 0)),
				   new TextureMap.Face(new Vector2Int(6, 0)),
				   new TextureMap.Face(new Vector2Int(6, 0)),
				   new TextureMap.Face(new Vector2Int(6, 0)),
				   new TextureMap.Face(new Vector2Int(6, 0)),
				   new TextureMap.Face(new Vector2Int(6, 0))
			   )
		   }, {
			   Blocks.Snow, new TextureMap(
				   new TextureMap.Face(new Vector2Int(0, 6)),
				   new TextureMap.Face(new Vector2Int(0, 6)),
				   new TextureMap.Face(new Vector2Int(0, 6)),
				   new TextureMap.Face(new Vector2Int(0, 6)),
				   new TextureMap.Face(new Vector2Int(0, 6)),
				   new TextureMap.Face(new Vector2Int(0, 6))
			   )
		   }, {
			   Blocks.Cobblestone, new TextureMap(
				   new TextureMap.Face(new Vector2Int(2, 4)),
				   new TextureMap.Face(new Vector2Int(2, 4)),
				   new TextureMap.Face(new Vector2Int(2, 4)),
				   new TextureMap.Face(new Vector2Int(2, 4)),
				   new TextureMap.Face(new Vector2Int(2, 4)),
				   new TextureMap.Face(new Vector2Int(2, 4))
			   )
		   }
	   };
	}

	public class TextureMap {
		public TextureMap(Face front, Face back, Face left, Face right, Face top, Face bottom) {
			this.front = front;
			this.back = back;
			this.left = left;
			this.right = right;
			this.top = top;
			this.bottom = bottom;
		}

		public readonly Face front, back, left, right, top, bottom;

		public class Face {
			public Face(Vector2Int tl) {
				this.tl = tl;
				tr = tl + new Vector2Int(1, 0);
				bl = tl + new Vector2Int(0, 1);
				br = tl + new Vector2Int(1, 1);
			}

			public Vector2Int tl, tr, bl, br;
		}
	}
}