using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PromtInputBotDemo
{
    public class ConversationFlow
    {
        public enum QuestionState
        {
            IsAcceptBooking,
            EmployeeId,
            RoomNo,
            TimeFrom,
            TimeTo,
            None
        }
        public QuestionState LastQuestionState { get; set; } = QuestionState.None;
    }
}
