"""Rebuild original element cues with Python's standard library; no external samples."""
import math
from pathlib import Path
import random
import struct
import wave

RATE = 44100
OUTPUT = Path(__file__).resolve().parents[1] / "src/Sts2Mod/Sts2Mod/audio"


def samples(kind, duration):
    rng = random.Random(20261028)
    low = 0.0
    previous = 0.0
    result = []
    for i in range(round(RATE * duration)):
        t = i / RATE
        noise = rng.uniform(-1, 1)
        low += 0.07 * (noise - low)
        attack = min(1, t / 0.012)
        release = min(1, (duration - t) / 0.05)
        if kind == "wind":
            value = (0.8 * low + 0.06 * math.sin(2 * math.pi * (650 * t + 900 * t * t))) * math.exp(-4 * t)
        elif kind == "rock":
            value = (0.45 * math.sin(2 * math.pi * (95 * t + 10 * (1 - math.exp(-16 * t))))
                     + 0.45 * low + 0.12 * noise * math.exp(-35 * t)) * math.exp(-10 * t)
        elif kind == "lightning":
            value = (0.17 * (noise - previous) * math.exp(-18 * t)
                     + 0.13 * math.sin(2 * math.pi * 1568 * t) * math.exp(-8 * t)
                     + 0.09 * math.sin(2 * math.pi * 784 * t) * math.exp(-6 * t))
        else:
            value = 0.0
            for delay, frequency in ((0, 523.25), (0.055, 659.25), (0.11, 783.99)):
                elapsed = t - delay
                if elapsed >= 0:
                    value += 0.16 * min(1, elapsed / 0.008) * math.exp(-7 * elapsed) * math.sin(2 * math.pi * frequency * elapsed)
        previous = noise
        result.append(value * attack * release)
    peak = max(abs(v) for v in result)
    # Keep every cue below -9 dBFS; no hard clipping or click at either endpoint.
    gain = min(1, 0.35 / peak)
    return [round(v * gain * 32767) for v in result]


def main():
    OUTPUT.mkdir(parents=True, exist_ok=True)
    for kind, duration in (("wind", 0.36), ("rock", 0.32), ("lightning", 0.30), ("combo", 0.52)):
        data = samples(kind, duration)
        assert data == samples(kind, duration), "Audio must be reproducible"
        assert max(abs(v) for v in data) <= 11469 and max(abs(v) for v in data) > 500
        assert data[0] == 0 and abs(data[-1]) < 20, "Cue endpoints must not click"
        path = OUTPUT / f"{kind}.wav"
        with wave.open(str(path), "wb") as stream:
            stream.setparams((1, 2, RATE, 0, "NONE", "not compressed"))
            stream.writeframes(struct.pack(f"<{len(data)}h", *data))
        with wave.open(str(path), "rb") as stream:
            assert stream.getnchannels() == 1 and stream.getsampwidth() == 2
            assert stream.getnframes() == len(data) and stream.getframerate() == RATE
        print(f"PASS {path.name}: {duration:.2f}s peak={max(abs(v) for v in data) / 32767:.3f}")


if __name__ == "__main__":
    main()
