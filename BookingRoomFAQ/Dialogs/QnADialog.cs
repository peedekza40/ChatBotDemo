// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Designer.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using QnAPrompting.Helpers;
using QnAPrompting.Models;

namespace QnAPrompting.Dialogs
{
    public class QnADialog : FunctionDialogBase
    {
        private IQnAService _qnaService;

        public QnADialog(IQnAService qnaService)
            : base(nameof(QnADialog))
        {
            _qnaService = qnaService;
        }

        protected override async Task<(object newState, IEnumerable<Activity> output, object result)> ProcessAsync(object oldState, Activity inputActivity)
        {
            var resultActivity = new List<Activity>();
            QnABotState newState = null;
            
            var query = inputActivity.Text;           
            var qnaResult = await _qnaService.QueryQnAServiceAsync(query, (QnABotState)oldState);
            var qnaAnswer = qnaResult[0].Answer;
            var prompts = qnaResult[0].Context?.Prompts;
            if (prompts == null || prompts.Length < 1)
            {
                if (qnaAnswer == "No good match found in KB.")
                {
                    resultActivity.Add(MessageFactory.Text("ขออภัยค่ะ ฉันไม่เข้าใจ"));
                    resultActivity.Add(MessageFactory.Text("พิมพ์ \"FAQ\" เพื่อกลับสู่บริการตอบคำถาม"));
                }
                else
                {
                    resultActivity.Add(MessageFactory.Text(qnaAnswer));
                    resultActivity.Add(MessageFactory.Text("พิมพ์ \"ถามอีกครั้ง\" หรือ \"ถามอีกครั้งได้ไหม\" เพื่อถามเรื่องอื่น ๆ"));
                }
            }
            else
            {
                // Set bot state only if prompts are found in QnA result
                newState = new QnABotState
                {
                    PreviousQnaId = qnaResult[0].Id,
                    PreviousUserQuery = query
                };

                resultActivity.Add(CardHelper.GetHeroCard(qnaAnswer, prompts));
            }

            return (newState, resultActivity, null);
        }
    }
}
