/*
 * Syntax highlighting by Parsing the RTF file format.
 * Sample program.
 * Copyright Alun Evans 2006
 * */

using System.Windows.Forms;

namespace HtmlRenderer.Demo.WinForms
{
    public static class SyntaxHilight
    {
        public static void AddColoredText(string strTextToAdd, RichTextBox rtb)
        {
            var selectionStart = rtb.SelectionStart;
            
            //Use the RichTextBox to create the initial RTF code
            rtb.Clear();
            rtb.AppendText(strTextToAdd);
            string strRtf = rtb.Rtf;
            rtb.Clear();

            /* 
             * ADD COLOUR TABLE TO THE HEADER FIRST 
             * */

            // Search for color table info, if it exists (which it shouldn't)
            // remove it and replace with our one
            int iCTableStart = strRtf.IndexOf("colortbl;");

            if (iCTableStart != -1) //then colortbl exists
            {
                //find end of colortbl tab by searching
                //forward from the colortbl tab itself
                int iCTableEnd = strRtf.IndexOf('}', iCTableStart);
                strRtf = strRtf.Remove(iCTableStart, iCTableEnd - iCTableStart);

                //now insert new colour table at index of old colortbl tag
                strRtf = strRtf.Insert(iCTableStart,
                    // CHANGE THIS STRING TO ALTER COLOUR TABLE
                    "colortbl ;\\red255\\green0\\blue0;\\red0\\green128\\blue0;\\red0\\green0\\blue255;}");
            }

            //colour table doesn't exist yet, so let's make one
            else
            {
                // find index of start of header
                int iRTFLoc = strRtf.IndexOf("\\rtf");
                // get index of where we'll insert the colour table
                // try finding opening bracket of first property of header first                
                int iInsertLoc = strRtf.IndexOf('{', iRTFLoc);

                // if there is no property, we'll insert colour table
                // just before the end bracket of the header
                if (iInsertLoc == -1) iInsertLoc = strRtf.IndexOf('}', iRTFLoc) - 1;

                // insert the colour table at our chosen location                
                strRtf = strRtf.Insert(iInsertLoc,
                    // CHANGE THIS STRING TO ALTER COLOUR TABLE
                    "{\\colortbl ;\\red128\\green0\\blue0;\\red240\\green0\\blue0;\\red0\\green128\\blue0;\\red0\\green0\\blue255;\\red128\\green0\\blue171;}");
            }

            /*
             * NOW PARSE THROUGH RTF DATA, ADDING RTF COLOUR TAGS WHERE WE WANT THEM
             * In our colour table we defined:
             * cf1 = red  
             * cf2 = green
             * cf3 = blue             
             * */

            bool inComment = false;
            bool inHtmlTag = false;
            bool inAttributeVal = false;
            for (int i = 0; i < strRtf.Length; i++)
            {
                if (!inComment && strRtf[i] == '<')
                {
                    //add RTF tags after symbol 
                    //Check for comments tags 
                    if (strRtf[i + 1] == '!' && strRtf[i + +2] == '-')
                    {
                        strRtf = strRtf.Insert(i, "\\cf3 ");
                        inComment = true;
                    }
                    else
                    {
                        strRtf = strRtf.Insert(i + 1, "\\cf1 ");
                        inHtmlTag = true;
                    }
                    //add RTF before symbol
                    strRtf = strRtf.Insert(i, "\\cf4 ");

                    //skip forward past the characters we've just added to avoid getting trapped in the loop
                    i += 10;
                }
                else if (strRtf[i] == '>')
                {
                    //add RTF tags after character
                    //Check for comments tags
                    if (strRtf[i - 1] == '-')
                    {
                        strRtf = strRtf.Insert(i + 1, "\\cf0 ");
                        strRtf = strRtf.Insert(i + 1, "\\cf4 ");
                        inComment = false;
                        //skip forward past the 6 characters we've just added
                        i += 8;
                    }
                    else if(!inComment)
                    {
                        strRtf = strRtf.Insert(i + 1, "\\cf0 ");
                        strRtf = strRtf.Insert(i, "\\cf4 ");
                        //skip forward past the 6 characters we've just added
                        i += 6;
                    }
                    inHtmlTag = false;
                    inAttributeVal = false;
                }
                else if (inHtmlTag && !inAttributeVal && strRtf[i] == ' ')
                {
                    //add RTF tags after character
                    strRtf = strRtf.Insert(i + 1, "\\cf2 ");
                    i += 5;
                }
                else if (inHtmlTag && strRtf[i] == '/')
                {
                    //add RTF tags after character
                    strRtf = strRtf.Insert(i, "\\cf4");
                    strRtf = strRtf.Insert(i+5, "\\cf1 ");
                    i += 7;
                }
                else if (inHtmlTag && strRtf[i] == '=')
                {
                    //add RTF tags after character
                    strRtf = strRtf.Insert(i, "\\cf4 ");
                    strRtf = strRtf.Insert(i+6, "\\cf5 ");
                    i += 10;
                }
                else if (inHtmlTag && strRtf[i] == '"')
                {
                    //add RTF tags after character
                    strRtf = strRtf.Insert(i, "\\cf4 ");
                    strRtf = strRtf.Insert(i+6, "\\cf5 ");
                    i += 10;
                    inAttributeVal = !inAttributeVal;
                }
            }
            rtb.Rtf = strRtf;
            rtb.SelectionStart = selectionStart;
        }
    }
}
