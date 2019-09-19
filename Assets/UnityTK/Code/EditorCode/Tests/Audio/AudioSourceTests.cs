using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityTK.Audio;

namespace UnityTK.Test.Audio
{
	public class AudioSourceTests
	{
		[Test]
		public void TestVolume()
		{
			// Arrange
			GameObject go = new GameObject();
			var src = go.AddComponent<AudioSource>();
			var uSrc = go.AddComponent<UTKAudioSource>();

			// Act
			uSrc.volume = .75f;
			uSrc.clipVolume = .25f;

			// Assert
			Assert.AreEqual(.75f * .25f, src.volume);

			// Act
			uSrc.clipVolume = .25f;
			uSrc.volume = .75f;

			// Assert
			Assert.AreEqual(.75f * .25f, src.volume);
		}

		[Test]
		public void TestPitch()
		{
			// Arrange
			GameObject go = new GameObject();
			var src = go.AddComponent<AudioSource>();
			var uSrc = go.AddComponent<UTKAudioSource>();

			// Act
			uSrc.pitch = .75f;
			uSrc.clipPitch = .25f;

			// Assert
			Assert.AreEqual(.75f * .25f, src.pitch);

			// Act
			uSrc.clipPitch = .25f;
			uSrc.pitch = .75f;

			// Assert
			Assert.AreEqual(.75f * .25f, src.pitch);
		}
	}
}