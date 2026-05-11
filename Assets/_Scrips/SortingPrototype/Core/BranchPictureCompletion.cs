using System;

namespace SortingPrototype.Core
{
    public static class BranchPictureCompletion
    {
        public const int PictureWidth = 4;

        public static bool IsPicture4x1Complete(BranchState branch)
        {
            if (branch == null)
            {
                throw new ArgumentNullException(nameof(branch));
            }

            if (!branch.IsUniformAndFull() || branch.PieceCount != PictureWidth)
            {
                return false;
            }

            var seenMask = 0;
            for (var i = 0; i < branch.Pieces.Count; i++)
            {
                var variant = branch.Pieces[i].Variant;
                if (variant < 0 || variant >= PictureWidth)
                {
                    return false;
                }

                var bit = 1 << variant;
                if ((seenMask & bit) != 0)
                {
                    return false;
                }

                seenMask |= bit;
            }

            return seenMask == 0b1111;
        }
    }
}

