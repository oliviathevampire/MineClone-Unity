using VampireStudios;

public class Materials {

	public static readonly Material STONE = new Material().SetReplaceable(false).SetSolid(true)
														  .SetDigSound(AudioManager.Dig.Type.Stone);
	public static readonly Material AIR = new Material().SetReplaceable(true).SetSolid(false)
														  .SetDigSound(AudioManager.Dig.Type.Silent);
	public static readonly Material WOOD = new Material().SetReplaceable(false).SetSolid(true)
														  .SetDigSound(AudioManager.Dig.Type.Wood);
	public static readonly Material ORGANIC = new Material().SetReplaceable(false).SetSolid(true)
														 .SetDigSound(AudioManager.Dig.Type.Gravel);
	public static readonly Material GRASS = new Material().SetReplaceable(false).SetSolid(true)
														 .SetDigSound(AudioManager.Dig.Type.Grass);
	public static readonly Material LEAVES = new Material().SetReplaceable(true).SetSolid(true)
														  .SetDigSound(AudioManager.Dig.Type.Grass);
}