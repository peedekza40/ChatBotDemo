// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DialogDemoBot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples
{
    public class BookingDataDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<BookingData> _userProfileAccessor;

        public BookingDataDialog(UserState userState)
            : base(nameof(BookingDataDialog))
        {
            _userProfileAccessor = userState.CreateProperty<BookingData>("BookingData");

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                AllowBookingStepAsync,
                SelectRoomStepAsync,
                EmployeeIdStepAsync,
                BookingDateStepAsync,
                TimeFromStepAsync,
                TimeToStepAsync,
                ConfirmBookingStepAsync,
                FinalStepAsync
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new DateTimePrompt(nameof(DateTimePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AllowBookingStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"ยินดีต้อนรับสู่ตัวช่วยในการจองห้องประชุม"), cancellationToken);
            return await stepContext.PromptAsync(nameof(ConfirmPrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("คุณต้องการที่จะทำการจองห้องหรือไม่")
                }, cancellationToken);
        }

        private static async Task<DialogTurnResult> SelectRoomStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("โปรดเลือกห้องที่ต้องการจอง"),
                        RetryPrompt = MessageFactory.Text("กรุณาเลือกอีกครั้ง"),
                        Choices = ChoiceFactory.ToChoices(new List<string> { "Room 1", "Room 2", "Room 3", "Room 4" }),
                    }, cancellationToken);
            }
            else
            {
                return await stepContext.EndDialogAsync();
            }
            
        }

        private static async Task<DialogTurnResult> EmployeeIdStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var previousResult = ((FoundChoice)stepContext.Result).Value;
            switch (previousResult)
            {
                case "Room 1":
                    stepContext.Values["AssetId"] = 1;
                    break;
                case "Room 2":
                    stepContext.Values["AssetId"] = 2;
                    break;
                case "Room 3":
                    stepContext.Values["AssetId"] = 3;
                    break;
                case "Room 4":
                    stepContext.Values["AssetId"] = 4;
                    break;
            }
            return await stepContext.PromptAsync(nameof(TextPrompt), 
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("โปรดใส่รหัสพนักงาน")
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> BookingDateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["EmployeeId"] = (string)stepContext.Result;
            return await stepContext.PromptAsync(nameof(DateTimePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("วันที่คุณต้องการจองคือวันที่เท่าไหร่"),
                    RetryPrompt = MessageFactory.Text("กรุณาใส่วันที่ให้ถูกต้อง")
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> TimeFromStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var previousResult = (stepContext.Result as IList<DateTimeResolution>)?.FirstOrDefault().Value;
            var bookingDate = DateTime.Parse(previousResult);
            stepContext.Values["BookingDate"] = bookingDate;
            return await stepContext.PromptAsync(nameof(DateTimePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("เริ่มจองตั้งแต่เวลา ?"),
                    RetryPrompt = MessageFactory.Text("กรุณาใส่เวลาให้ถูกต้อง")
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> TimeToStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var previousResult = (stepContext.Result as IList<DateTimeResolution>)?.FirstOrDefault().Value;
            var timeFrom = DateTime.Parse(previousResult);
            stepContext.Values["TimeFrom"] = timeFrom;
            return await stepContext.PromptAsync(nameof(DateTimePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("จองถึงเวลา ?"),
                    RetryPrompt = MessageFactory.Text("กรุณาใส่เวลาให้ถูกต้อง")
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmBookingStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var previousResult = (stepContext.Result as IList<DateTimeResolution>)?.FirstOrDefault().Value;
            var timeTo = DateTime.Parse(previousResult).TimeOfDay;

            var bookingData = new BookingData();
            bookingData.AssetId = (int)stepContext.Values["AssetId"];
            bookingData.EmployeeId = (string)stepContext.Values["EmployeeId"];
            bookingData.BookingDate = (DateTime)stepContext.Values["BookingDate"];
            bookingData.TimeFrom = ((DateTime)stepContext.Values["TimeFrom"]).TimeOfDay;
            bookingData.TimeTo = timeTo;

            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);
            var listFact = new List<Fact>();
            listFact.Add(new Fact("ห้องที่", bookingData.AssetId.ToString()));
            listFact.Add(new Fact("Employee Id", bookingData.EmployeeId));
            listFact.Add(new Fact("วันที่จอง", bookingData.BookingDate.ToString("dd MMM yyyy")));
            listFact.Add(new Fact("ช่วงเวลาที่จอง", $"{bookingData.TimeFrom.ToString(@"hh\:mm")} - {bookingData.TimeTo.ToString(@"hh\:mm")}"));
            var summaryBooking = new ReceiptCard
            {
                Title = "ยืนยันการจอง",
                Facts = listFact
            };

            //Line and Messengers can't show receipt card
            reply.Attachments.Add(summaryBooking.ToAttachment());

            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("ข้อมูลถูกต้องหรือไม่ ?")
                }, cancellationToken);
        }
        
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("ทำการจองสำเร็จ"), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("ยกเลิกขั้นตอนการจอง"), cancellationToken);
            }
            return await stepContext.EndDialogAsync();
        }
    }
}
