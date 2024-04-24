public class MathUtility {
    public static float MapValue(float input, float inputStart, float inputEnd, float outputStart, float outputEnd) {
        if (input < inputStart)
            input = inputStart;
        if (input > inputEnd)
            input = inputEnd;
        return outputStart + ((outputEnd - outputStart) / (inputEnd - inputStart)) * (input - inputStart);
    }
}
