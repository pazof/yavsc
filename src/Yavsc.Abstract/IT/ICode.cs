using System.Collections.Generic;

namespace Yavsc.Abstract.IT
{
    // un code est, parmis les ensembles de suites de signes,
    // ceux qui n'ont qu'une seule suite de suites pouvant représenter toute suite de suite de signes

    public interface ICode<TSign> : IEnumerable<IEnumerable<TSign>>
    {
        /// <summary>
        /// Checks false that a letter list combinaison correspond to another one
        /// </summary>
        /// <returns></returns>
        bool Validate();

        /// <summary>
        /// Defines a new letter in this code, 
        /// as an enumerable of <c>TLetter</c>
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        void AddLetter(IEnumerable<TSign> letter);
    }
}
