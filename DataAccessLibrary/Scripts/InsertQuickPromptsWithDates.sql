-- INSERT statements with explicit date values for QuickPrompts table
-- This version explicitly sets CreatedDate to avoid any default value issues

INSERT INTO [dbo].[QuickPrompts] ([Type], [Command], [PromptText], [Description], [CreatedDate], [IsActive]) VALUES 
-- FIXES
('FIXES', 'fix grammar formally', 'Fix any mistakes or irregularities in grammar, spelling, or formatting. Use a professional business tone. The text was created using voice dictation. Thus, there are likely to be issues regarding homophones and other misrecognitions. Do not change the original structure of the text.', 'Fix grammar with professional tone', GETUTCDATE(), 1),
('FIXES', 'fix grammar', 'Fix any mistakes or irregularities in grammar, spelling, or formatting. The text was created using voice dictation. Thus, there are likely to be issues regarding homophones and other misrecognitions. Do not change the tone. Do not change the original structure of the text.', 'Fix grammar maintaining original tone', GETUTCDATE(), 1),
('FIXES', 'fix syntax', 'Fix any syntax errors in this code selection. Do not change any behavior.', 'Fix code syntax errors only', GETUTCDATE(), 1),

-- FORMATTING
('FORMATTING', 'format table', 'The following markdown text is raw data. There is no index. Return the text in a markdown table format. Each row has a new line in the original data.', 'Convert raw data to markdown table', GETUTCDATE(), 1),
('FORMATTING', 'format bullets', 'Convert each paragraph into a heading with a series of bullet points underneath it. Each paragraph is separated by a new line. Separate paragraphs should not have combined bullet points. This should all be done in markdown syntax. If it is a small paragraph, then you can just leave it as a heading and not add bullet points. Do not reduce content, only reduce things that would be redundant. These bullet points should be in a useful format for notes for those who want to quickly look at it. If there is a citation in the markdown original, then keep the citation just at the top and not within every individual bullet point.', 'Convert to headings with bullet points', GETUTCDATE(), 1),
('FORMATTING', 'format mermaid', 'Convert the following plain text into the text syntax for a mermaid diagram.', 'Convert to mermaid diagram syntax', GETUTCDATE(), 1),
('FORMATTING', 'format comment', 'Format the following text as a comment for the current programming language. Use the proper comment syntax for the current language. Split the comment into multiple lines if the lines are too long.', 'Format as programming language comment', GETUTCDATE(), 1),
('FORMATTING', 'group', 'Act as an organizer. The following text consists of various topics all put together. Please group these items into categories and label each category. Return just the results.', 'Group and categorize items', GETUTCDATE(), 1),
('FORMATTING', 'join', 'Act as an editor. The following text is separated into multiple parts. Please group them together into one part maintaining the flow and meaning. Reorder in whatever way makes sense. Remove any redundant information. The result should be only one part with no additional structure. Return just the modified text.', 'Join separated text parts', GETUTCDATE(), 1),

-- TEXT GENERATION
('TEXT GENERATION', 'explain', 'Explain this text in a way that is easier to understand for a layman without technical knowledge.', 'Explain in simple terms', GETUTCDATE(), 1),
('TEXT GENERATION', 'summarize', 'Summarize this text into a format suitable for project notes.', 'Create project notes summary', GETUTCDATE(), 1),
('TEXT GENERATION', 'add context', 'Add additional text to the selected text that would be appropriate to the situation and add useful information.', 'Add contextual information', GETUTCDATE(), 1),
('TEXT GENERATION', 'fit schema', 'The given text has a series of responses that need to be categorized. Each response has a key that needs to be mapped to a value. Infer the schema from the text unless it is given at the top of the text with prior examples. Return the key-value pairs in a JSON format unless you infer a different format.', 'Convert to schema format', GETUTCDATE(), 1),
('TEXT GENERATION', 'answer', 'Generate text that satisfies the question or request given in the input.', 'Generate answer to question', GETUTCDATE(), 1),
('TEXT GENERATION', 'shell', 'Generate a shell script that performs the following actions. Output only the command. Do not output any comments or explanations. Default to the bash shell unless otherwise specified.', 'Generate shell script', GETUTCDATE(), 1),
('TEXT GENERATION', 'add emoji', 'Return the same exact text verbatim with the same formatting, but add emoji when appropriate in order to make the text fun and easier to understand.', 'Add appropriate emojis', GETUTCDATE(), 1),
('TEXT GENERATION', 'make softer', 'Act as an editor. I want you to make the following text softer in tone. Return just the modified text.', 'Soften text tone', GETUTCDATE(), 1),
('TEXT GENERATION', 'make stronger', 'Act as an editor. I want you to make the following text stronger in tone. Return just the modified text.', 'Strengthen text tone', GETUTCDATE(), 1),

-- FILE CONVERSIONS
('FILE CONVERSIONS', 'convert to jason', 'Convert the following data into a JSON format.', 'Convert to JSON format', GETUTCDATE(), 1),
('FILE CONVERSIONS', 'convert to markdown', 'Convert the following text into a markdown format.', 'Convert to markdown', GETUTCDATE(), 1),
('FILE CONVERSIONS', 'convert to python', 'Convert the following key-value pairs into the syntax for a Python dictionary. So you should serialize the key-value pairs into a native Python format.', 'Convert to Python dictionary', GETUTCDATE(), 1),
('FILE CONVERSIONS', 'convert to sheet', 'Convert the following data into a CSV format.', 'Convert to CSV format', GETUTCDATE(), 1),
('FILE CONVERSIONS', 'convert to yam', 'Convert the following data into a YAML format.', 'Convert to YAML format', GETUTCDATE(), 1),

-- CHECKERS
('CHECKERS', 'describe code', 'Explain what the following code does in natural language at a high level without getting into the specifics of the syntax.', 'Describe code functionality', GETUTCDATE(), 1),
('CHECKERS', 'check grammar', 'Check the grammar and formatting of the following text. Return a list of all potential errors.', 'Check for grammar errors', GETUTCDATE(), 1),
('CHECKERS', 'check spelling', 'Check the spelling of the following text. Return a list of all potential errors.', 'Check for spelling errors', GETUTCDATE(), 1),
('CHECKERS', 'check structure', 'Skim the structure and layout of the following text. Tell me if the structure and order of my writing are correct. If it is not correct or flows poorly, then tell me what might be wrong with it. If it is all correct, then say it looks good.', 'Check text structure and flow', GETUTCDATE(), 1),

-- TRANSLATIONS
('TRANSLATIONS', 'translate to english', 'Translate the following text into English.', 'Translate text to English', GETUTCDATE(), 1),

-- CODE GENERATION
('CODE GENERATION', 'generate code', 'The following plaintext describes a process in code in the language that is specified by the system prompt. Please output the code necessary to do this. Return just code and not any natural language explanations.', 'Generate code from description', GETUTCDATE(), 1),
('CODE GENERATION', 'update comments', 'Act as a software engineer. The following code may be missing comments or the comments could be out of date. Please update the comments. If you are unsure how to comment something, ask a question in a comment instead. Return just the code and not any explanations.', 'Update code comments', GETUTCDATE(), 1),
('CODE GENERATION', 'clean code', 'Act as a software engineer. Reduce any duplication in the selected code and improve it to be more idiomatic and clear for other users. However, do not change the behavior or functionality. Return just the code and not any explanations.', 'Clean and improve code', GETUTCDATE(), 1),
('CODE GENERATION', 'improve semantics', 'The following is an HTML document. Keep the same structure and layout but if it is needed, change any elements to use proper semantic HTML and make sure it is implementing best practices for user accessibility. Output just the HTML and not any extra explanations.', 'Improve HTML semantics', GETUTCDATE(), 1),

-- WRITING HELPERS
('WRITING HELPERS', 'add questions', 'Help me explore this question from multiple perspectives. For each perspective, ask follow-up questions and indicate what perspective is being taken.', 'Explore multiple perspectives', GETUTCDATE(), 1),
('WRITING HELPERS', 'format outline', 'Create an outline that encapsulates the text below. Keep the number of sections between three and five to optimize for human working memory. Return just the outline.', 'Create structured outline', GETUTCDATE(), 1),
('WRITING HELPERS', 'format prose', 'As an editor, format the following outline or summarization as prose. You can have headings and paragraphs. Avoid using bullet points. Reorder and add transitions as necessary to make the document flow. Return just the text.', 'Convert outline to prose', GETUTCDATE(), 1);
