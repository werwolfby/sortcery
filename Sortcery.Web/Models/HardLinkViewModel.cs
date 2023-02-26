using System.Collections.Generic;
using Sortcery.Api.Contracts.Models;

namespace Sortcery.Web.Models
{
    public class HardLinkViewModel
    {
        private readonly HardLinkData _hardLinkData;

        public HardLinkViewModel(HardLinkData hardLinkData)
        {
            _hardLinkData = hardLinkData;
        }

        public FileData? Source => _hardLinkData.Source;

        public List<FileData> Targets => _hardLinkData.Targets;

        public FileData? Guess { get; set; }

        public bool Guessing { get; set; }

        public bool Editing { get; set; }

        public bool CanGuess => _hardLinkData.Targets.Count == 0 && Guess == null;

        public bool CanLink => Guess != null;

        public IEnumerable<FileData> AllTargets
        {
            get
            {
                foreach (var target in Targets)
                {
                    yield return target;
                }

                if (Guess != null)
                {
                    yield return Guess;
                }
            }
        }
    }
}
