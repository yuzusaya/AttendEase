using AttendEase.Shared.Utilities;

namespace AttendEase.UnitTest
{
    public class TimeUtilityTest
    {
        [Theory]
        [InlineData(8,40,8,50)]
        [InlineData(8,41,8,50)]
        [InlineData(8,42,8,50)]
        [InlineData(8,43,8,55)]
        [InlineData(8,44,8,55)]
        [InlineData(8,45,8,55)]
        [InlineData(8,46,8,55)]
        [InlineData(8,47,8,55)]
        [InlineData(8,48,9,0)]
        [InlineData(8,49,9,0)]
        [InlineData(8,50,9,0)]
        [InlineData(8,51,9,0)]
        [InlineData(8,52,9,0)]
        [InlineData(8,53,9,05)]
        [InlineData(8,54,9,05)]
        [InlineData(8,55,9,05)]
        public void GetCheckInTimeFromStartTime_ShouldReturnsEarliestTime(int startHour,int startMin,int expectedCheckInHour,int expectedCheckInMin)
        {
            TimeOnly startTime = new TimeOnly(startHour, startMin);
            var checkInTime = TimeUtility.GetCheckInTimeFromStartTime(startTime);
            var expectedCheckInTime = new TimeOnly(expectedCheckInHour, expectedCheckInMin);
            Assert.Equal(expectedCheckInTime, checkInTime);
        }

        [Theory]
        [InlineData(17, 30, 17, 20)]
        [InlineData(17, 31, 17, 20)]
        [InlineData(17, 32, 17, 20)]
        [InlineData(17, 33, 17, 25)]
        [InlineData(17, 34, 17, 25)]
        [InlineData(17, 35, 17, 25)]
        [InlineData(17, 36, 17, 25)]
        [InlineData(17, 37, 17, 25)]
        [InlineData(17, 38, 17, 30)]
        [InlineData(17, 39, 17, 30)]
        [InlineData(17, 40, 17, 30)]
        [InlineData(17, 41, 17, 30)]
        [InlineData(17, 42, 17, 30)]
        [InlineData(17, 43, 17, 35)]
        [InlineData(17, 44, 17, 35)]
        [InlineData(17, 45, 17, 35)]
        public void GetCheckOutTimeFromEndTime_ShouldReturnsLatestTime(int endHour, int endMin, int expectedCheckOutHour, int expectedCheckOutMin)
        {
            TimeOnly endTime = new TimeOnly(endHour, endMin);
            var checkOutTime = TimeUtility.GetCheckOutTimeFromEndTime(endTime);
            var expectedCheckOutTime = new TimeOnly(expectedCheckOutHour, expectedCheckOutMin);
            Assert.Equal(expectedCheckOutTime, checkOutTime);
        }
    }
}