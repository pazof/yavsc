using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Process
{
    /// <summary>
    /// An abstract, identified rule
    /// </summary>
    public class Rule<TResult,TInput>
    {
        [Key]
        public string Id { get; set; }

        /// <summary>
        /// Left part for this rule, a conjonction.
        /// All of these requisitions must be true
        /// in order to begin any related process.
        /// </summary>
        /// <returns></returns>
        public Conjonction Left { get; set; }
        /// <summary>
        /// Right part of this rule, a disjonction.
        /// That is, only one of these post requisitions
        /// has to be true in order for this rule
        /// to expose a success.
        /// </summary>
        /// <returns></returns>
        public Disjonction Right { get; set; }

        public string Description { get; set; }

        public Action<TResult,TInput> Execution { get; set; }

    }
}
