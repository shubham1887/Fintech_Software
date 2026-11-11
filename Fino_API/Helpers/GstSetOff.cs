namespace FintechApp.Helpers
{
    /// <summary>
    /// How input credit is used against output tax — the order in section 49 / rule 88A.
    ///
    /// The return used to subtract credit head by head and clamp each at zero. That is not how
    /// credit works: credit under IGST pays IGST first and whatever is left pays CGST and SGST,
    /// and CGST or SGST credit left after its own head may pay IGST. On an install whose
    /// suppliers bill IGST and whose customers are mostly in its own state, the head-by-head
    /// reading shows the whole CGST + SGST as payable in cash beside lakhs of unused IGST credit.
    ///
    /// The one rule that is not symmetric: CGST credit never pays SGST and SGST credit never
    /// pays CGST. Cess stands entirely alone.
    /// </summary>
    public static class GstSetOff
    {
        public readonly record struct Heads(decimal IGST, decimal CGST, decimal SGST, decimal Cess)
        {
            public decimal Total => IGST + CGST + SGST + Cess;
            public static Heads operator +(Heads a, Heads b) =>
                new(a.IGST + b.IGST, a.CGST + b.CGST, a.SGST + b.SGST, a.Cess + b.Cess);
            public static readonly Heads Zero = new(0m, 0m, 0m, 0m);
        }

        /// <param name="Payable">What is left to pay in cash, per head.</param>
        /// <param name="CreditLeft">Credit not used this month — it carries to the next.</param>
        /// <param name="CreditUsed">Credit consumed, per head of CREDIT (not of liability).</param>
        public readonly record struct Result(Heads Payable, Heads CreditLeft, Heads CreditUsed);

        public static Result Apply(Heads output, Heads credit)
        {
            // A negative "credit" is a reversal bigger than the month's claims: it is tax owed,
            // so it joins the liability of its own head instead of being silently dropped.
            decimal lI = Math.Max(0m, output.IGST) + Math.Max(0m, -credit.IGST);
            decimal lC = Math.Max(0m, output.CGST) + Math.Max(0m, -credit.CGST);
            decimal lS = Math.Max(0m, output.SGST) + Math.Max(0m, -credit.SGST);
            decimal lX = Math.Max(0m, output.Cess) + Math.Max(0m, -credit.Cess);

            decimal cI = Math.Max(0m, credit.IGST), cC = Math.Max(0m, credit.CGST),
                    cS = Math.Max(0m, credit.SGST), cX = Math.Max(0m, credit.Cess);
            decimal startI = cI, startC = cC, startS = cS, startX = cX;

            // IGST credit: IGST first, then CGST and SGST.
            Use(ref cI, ref lI); Use(ref cI, ref lC); Use(ref cI, ref lS);
            // CGST credit: CGST, then IGST. Never SGST.
            Use(ref cC, ref lC); Use(ref cC, ref lI);
            // SGST credit: SGST, then IGST. Never CGST.
            Use(ref cS, ref lS); Use(ref cS, ref lI);
            // Cess credit pays cess and nothing else.
            Use(ref cX, ref lX);

            return new Result(
                Payable: new Heads(R(lI), R(lC), R(lS), R(lX)),
                CreditLeft: new Heads(R(cI), R(cC), R(cS), R(cX)),
                CreditUsed: new Heads(R(startI - cI), R(startC - cC), R(startS - cS), R(startX - cX)));
        }

        private static void Use(ref decimal credit, ref decimal liability)
        {
            decimal used = Math.Min(credit, liability);
            credit -= used;
            liability -= used;
        }

        private static decimal R(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
    }
}
