// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.5.0

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;

namespace PromtInputBotDemo.Bots
{
    public class EchoBot : ActivityHandler
    {
        private BotState _conversationState;
        private BotState _bookingState;

        public EchoBot(ConversationState conversationState, UserState bookingState)
        {
            _conversationState = conversationState;
            _bookingState = bookingState;

        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var conversationStateAccessors = _conversationState.CreateProperty<ConversationFlow>(nameof(ConversationFlow));
            var flow = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationFlow());

            var bookingStateAccessors = _bookingState.CreateProperty<BookingData>(nameof(BookingData));
            var data = await bookingStateAccessors.GetAsync(turnContext, () => new BookingData());

            await FillOutBookingDataAsync(flow, data, turnContext, cancellationToken);
            
            // Save changes.
            await _conversationState.SaveChangesAsync(turnContext);
            await _bookingState.SaveChangesAsync(turnContext);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                var conversationStateAccessors = _conversationState.CreateProperty<ConversationFlow>(nameof(ConversationFlow));
                var flow = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationFlow());

                var bookingStateAccessors = _bookingState.CreateProperty<BookingData>(nameof(BookingData));
                var data = await bookingStateAccessors.GetAsync(turnContext, () => new BookingData());

                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"สวัสดี ! ยินดีต้อนรับสู่ตัวช่วยในการจองห้องประชุม"), cancellationToken);
                    await FillOutBookingDataAsync(flow, data, turnContext, cancellationToken);
                }

                // Save changes.
                await _conversationState.SaveChangesAsync(turnContext);
                await _bookingState.SaveChangesAsync(turnContext);
            }
        }

        private static async Task SendCardAsync(ITurnContext turnContext, List<CardAction> buttons,  CancellationToken cancellationToken)
        {
            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);
            var plCard = new HeroCard()
            {
                Title = "คุณต้องการที่จะทำการจองห้องหรือไม่ ?",
                Subtitle = string.Empty,
                Buttons = buttons
            };

            reply.Attachments.Add(plCard.ToAttachment());

            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        private static async Task FillOutBookingDataAsync(ConversationFlow flow, BookingData data, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            string input = turnContext.Activity.Text?.Trim();
            var buttons = new List<CardAction>()
                        {
                            new CardAction() { Title = "ใช่", Type = ActionTypes.ImBack, Value = "ใช่" },
                            new CardAction() { Title = "ไม่", Type = ActionTypes.ImBack, Value = "ไม่ใช่" }
                        };

            switch (flow.LastQuestionState)
            {
                case ConversationFlow.QuestionState.None:
                    await SendCardAsync(turnContext, buttons, cancellationToken);
                    flow.LastQuestionState = ConversationFlow.QuestionState.IsAcceptBooking;
                    break;
                case ConversationFlow.QuestionState.IsAcceptBooking:
                    if (turnContext.Activity.Text == "ใช่")
                    {
                        await turnContext.SendActivityAsync("โปรดใส่รหัสพนักงานของคุณ");
                        flow.LastQuestionState = ConversationFlow.QuestionState.EmployeeId;
                    }
                    else
                    {
                        await SendCardAsync(turnContext, buttons, cancellationToken);
                    }
                    break;
                case ConversationFlow.QuestionState.EmployeeId:
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        data.EmployeeId = input;
                        
                        flow.LastQuestionState = ConversationFlow.QuestionState.RoomNo;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync("เกิดข้อผิดพลาดกับการกรอกรหัสพนักงาน กรุณากรอกอีกครั้ง");
                    }
                    break;
                default:
                    await turnContext.SendActivityAsync(data.EmployeeId);
                    break;
            }
        }

        private IList<Choice> GetChoices()
        {
            var cardOptions = new List<Choice>()
            {
                new Choice() { Value = "Adaptive Card", Synonyms = new List<string>() { "adaptive" } },
                new Choice() { Value = "Animation Card", Synonyms = new List<string>() { "animation" } },
                new Choice() { Value = "Audio Card", Synonyms = new List<string>() { "audio" } },
                new Choice() { Value = "Hero Card", Synonyms = new List<string>() { "hero" } },
                new Choice() { Value = "Receipt Card", Synonyms = new List<string>() { "receipt" } },
                new Choice() { Value = "Signin Card", Synonyms = new List<string>() { "signin" } },
                new Choice() { Value = "Thumbnail Card", Synonyms = new List<string>() { "thumbnail", "thumb" } },
                new Choice() { Value = "Video Card", Synonyms = new List<string>() { "video" } },
                new Choice() { Value = "All cards", Synonyms = new List<string>() { "all" } },
            };

            return cardOptions;
        }



    }
}
