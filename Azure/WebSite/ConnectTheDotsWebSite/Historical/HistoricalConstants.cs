namespace Historical
{
    public static class HistoricalConstants
    {
        public const string STREAM_ANALYTICS_NAME_PREFIX = "historicalBlob";
        public const string BLOB_CONTAINER_NAME_PREFIX = "datablobs";
        public const string BLOB_NAME_PREFIX = "Temperature2T";
        public const int MAX_RESULT_SIZE = 500;
        public static int[] TUMBLING_WINDOW_SIZES_SEC = { 10 * 60 / MAX_RESULT_SIZE, 60 * 60 / MAX_RESULT_SIZE, 24 * 60 * 60 / MAX_RESULT_SIZE, 7 * 24 * 60 * 60 / MAX_RESULT_SIZE, 30 * 24 * 60 * 60 / MAX_RESULT_SIZE };
    }
}