using System.Collections.Generic;
using Sortcery.Api.Contracts.Models;

namespace Sortcery.Web.Models
{
    public class HardLinkViewModel
    {
        private readonly HardLinkInfo _hardLinkInfo;

        public HardLinkViewModel(HardLinkInfo hardLinkInfo)
        {
            _hardLinkInfo = hardLinkInfo;
        }

        public FileInfo? Source => _hardLinkInfo.Source;

        public List<FileInfo> Targets => _hardLinkInfo.Targets;

        public FileInfo? Guess { get; set; }

        public bool Guessing { get; set; }

        public bool CanGuess => _hardLinkInfo.Targets.Count == 0 && Guess == null;

        public bool CanLink => Guess != null;

        public IEnumerable<FileInfo> AllTargets
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
