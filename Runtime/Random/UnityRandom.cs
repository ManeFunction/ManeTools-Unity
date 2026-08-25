using System;
using System.Security.Cryptography;
using Mane.DotNet;
using Random = UnityEngine.Random;

namespace Mane.Unity
{
    /// <summary>
    /// <see cref="IRandom"/> implementation backed by <see cref="Random"/>.
    /// Each instance keeps its own <see cref="Random.State"/> so it does not
    /// disturb the global Unity RNG. The parameterless constructor seeds from
    /// a cryptographically strong source.
    /// </summary>
    public class UnityRandom : IRandom
    {
        private readonly int _seed;
        private Random.State _state;

        /// <inheritdoc />
        public int Seed => _seed;

        /// <summary>
        /// Creates a generator with the given seed.
        /// </summary>
        /// <param name="seed">Seed passed to <see cref="Random.InitState"/>.</param>
        public UnityRandom(int seed)
        {
            _seed = seed;
            InitFromSeed(seed);
        }

        /// <summary>
        /// Creates a generator seeded from <see cref="RandomNumberGenerator"/>.
        /// </summary>
        public UnityRandom()
        {
            Span<byte> bytes = stackalloc byte[4];
            RandomNumberGenerator.Fill(bytes);
            _seed = BitConverter.ToInt32(bytes);
            InitFromSeed(_seed);
        }

        /// <inheritdoc />
        public int Next(int min, int max)
        {
            if (min > max)
                throw new ArgumentOutOfRangeException(nameof(max));

            Random.State previous = Push();
            int result = Random.Range(min, max);
            Pop(previous);
            return result;
        }

        /// <inheritdoc />
        public double Range01Double()
        {
            Random.State previous = Push();
            double result = Random.Range(0, int.MaxValue) / (double)int.MaxValue;
            Pop(previous);
            return result;
        }

        /// <inheritdoc />
        public float Range01()
        {
            Random.State previous = Push();
            float result = Random.Range(0, int.MaxValue) * (1f / int.MaxValue);
            Pop(previous);
            return result;
        }

        private void InitFromSeed(int seed)
        {
            Random.State previous = Random.state;
            Random.InitState(seed);
            _state = Random.state;
            Random.state = previous;
        }

        private Random.State Push()
        {
            Random.State previous = Random.state;
            Random.state = _state;
            return previous;
        }

        private void Pop(Random.State previous)
        {
            _state = Random.state;
            Random.state = previous;
        }
    }
}
