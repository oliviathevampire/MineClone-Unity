namespace VampireStudios {
	public class Material {
		private bool _replaceable;
		private bool _solid;
		private AudioManager.Dig.Type _digSound;

		internal Material SetReplaceable(bool replaceable) {
			_replaceable = replaceable;
			return this;
		}

		internal Material SetSolid(bool solid) {
			_solid = solid;
			return this;
		}

		internal Material SetDigSound(AudioManager.Dig.Type digSound) {
			_digSound = digSound;
			return this;
		}

		public bool IsReplaceable() {
			return _replaceable;
		}

		public bool IsSolid() {
			return _solid;
		}

		public AudioManager.Dig.Type GetDigSound() {
			return _digSound;
		}
	
	}
}