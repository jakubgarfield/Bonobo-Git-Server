using Bonobo.Git.Server.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Bonobo.Git.Server.Helpers
{
    public static class RepositoryCommitModelHelpers
    {
        /// <summary>
        /// Create message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static RepositoryCommitTitleModel MakeCommitMessage(string message)
        {
            return BreakLine(message, 25);
        }

        /// <summary>
        /// Create message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageLengthLimit"></param>
        /// <returns></returns>
        internal static RepositoryCommitTitleModel MakeCommitMessage(string message, int messageLengthLimit)
        {
            return BreakLine(message, messageLengthLimit);
        }

        /// <summary>
        /// Split a string to blocks by word
        /// </summary>
        /// <param name="title"></param>
        /// <param name="blockLength"></param>
        /// <returns></returns>
        private static RepositoryCommitTitleModel BreakLine(string title, int blockLength)
        {
            IEnumerable<string> words = title.Split(' ')
                .Select(el => el.Trim())
                .Where(el => !string.IsNullOrEmpty(el)).ToArray();

            List<string> message = new List<string>();
            List<string> preBlock = new List<string>();
            bool addToPreMessage = false;

            int currentLen = 0;
            int wordsLength = words.Count();

            foreach (string word in words)
            {
                if (addToPreMessage)
                {
                    preBlock.Add(word);
                }
                else
                {
                    message.Add(word);
                }
                currentLen += word.Length;

                if (currentLen >= blockLength)
                {
                    currentLen = 0;
                    addToPreMessage = true;
                }
            }

            var messageString = string.Join(" ", message.ToArray()).Trim();
            var preBlockString = string.Join(" ", preBlock.ToArray()).Trim();

            return new RepositoryCommitTitleModel
            {
                ShortTitle = messageString,
                ExtraTitle = preBlockString
            };
        }
    }
}