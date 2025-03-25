using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media;
using System.Text;
using System.Text.RegularExpressions;

namespace AvaloniaChat
{
    public class MarkdownRenderer
    {
        public static Control Render(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return new TextBlock { Text = "" };

            var panel = new StackPanel
            {
                Spacing = 8
            };

            var lines = markdown.Split('\n');

            // 处理代码块
            bool inCodeBlock = false;
            StringBuilder codeBlockContent = new StringBuilder();
            string codeLanguage = string.Empty;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                // 处理代码块
                if (line.StartsWith("```"))
                {
                    if (!inCodeBlock)
                    {
                        inCodeBlock = true;
                        codeBlockContent.Clear();

                        // 提取语言
                        if (line.Length > 3)
                            codeLanguage = line.Substring(3).Trim();
                    }
                    else
                    {
                        // 代码块结束
                        inCodeBlock = false;
                        panel.Children.Add(CreateCodeBlock(codeBlockContent.ToString(), codeLanguage));
                        codeLanguage = string.Empty;
                    }
                    continue;
                }

                if (inCodeBlock)
                {
                    codeBlockContent.AppendLine(line);
                    continue;
                }

                // 处理标题
                if (line.StartsWith("# "))
                {
                    panel.Children.Add(CreateHeading(line.Substring(2), 1));
                }
                else if (line.StartsWith("## "))
                {
                    panel.Children.Add(CreateHeading(line.Substring(3), 2));
                }
                else if (line.StartsWith("### "))
                {
                    panel.Children.Add(CreateHeading(line.Substring(4), 3));
                }
                else if (line.StartsWith("#### "))
                {
                    panel.Children.Add(CreateHeading(line.Substring(5), 4));
                }
                else if (line.StartsWith("##### "))
                {
                    panel.Children.Add(CreateHeading(line.Substring(6), 5));
                }
                else if (line.StartsWith("###### "))
                {
                    panel.Children.Add(CreateHeading(line.Substring(7), 6));
                }
                // 处理无序列表
                else if (line.StartsWith("- ") || line.StartsWith("* "))
                {
                    panel.Children.Add(CreateListItem(line.Substring(2)));
                }
                // 处理有序列表
                else if (Regex.IsMatch(line, @"^\d+\.\s"))
                {
                    var match = Regex.Match(line, @"^\d+\.\s(.*)$");
                    if (match.Success)
                    {
                        panel.Children.Add(CreateListItem(match.Groups[1].Value, true));
                    }
                }
                // 处理引用块
                else if (line.StartsWith("> "))
                {
                    panel.Children.Add(CreateQuoteBlock(line.Substring(2)));
                }
                // 处理水平线
                else if (line.StartsWith("---") || line.StartsWith("***") || line.StartsWith("___"))
                {
                    panel.Children.Add(CreateHorizontalRule());
                }
                // 处理内联代码和其他格式的普通文本
                else
                {
                    panel.Children.Add(CreateParagraph(line));
                }
            }

            var scrollViewer = new ScrollViewer
            {
                Content = panel,
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
            };

            return scrollViewer;
        }

        private static Control CreateHeading(string text, int level)
        {
            var fontSize = 24 - ((level - 1) * 2);

            var textBlock = new TextBlock
            {
                FontSize = fontSize,
                FontWeight = FontWeight.Bold,
                Margin = new Thickness(0, 4 * (7 - level), 0, 2)
            };

            textBlock.Inlines = ParseInlineFormatting(text);

            return textBlock;
        }

        private static Control CreateParagraph(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new TextBlock { Height = 8 }; // 空行

            var textBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 4)
            };

            textBlock.Inlines = ParseInlineFormatting(text);

            return textBlock;
        }

        private static Control CreateListItem(string text, bool ordered = false)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(20, 0, 0, 4)
            };

            var bullet = new TextBlock
            {
                Text = ordered ? "• " : "• ",
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 0, 5, 0)
            };

            var content = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap
            };

            content.Inlines = ParseInlineFormatting(text);

            panel.Children.Add(bullet);
            panel.Children.Add(content);

            return panel;
        }

        private static Control CreateQuoteBlock(string text)
        {
            var border = new Border
            {
                BorderThickness = new Thickness(4, 0, 0, 0),
                BorderBrush = new SolidColorBrush(Colors.Gray),
                Padding = new Thickness(10, 5, 5, 5),
                Margin = new Thickness(0, 4, 0, 4),
                Background = new SolidColorBrush(Color.FromRgb(245, 245, 245))
            };

            var textBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap
            };

            textBlock.Inlines = ParseInlineFormatting(text);

            border.Child = textBlock;

            return border;
        }

        private static Control CreateCodeBlock(string code, string language)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 4, 0, 4)
            };

            var panel = new StackPanel();

            // 添加语言标签
            if (!string.IsNullOrEmpty(language))
            {
                panel.Children.Add(new TextBlock
                {
                    Text = language,
                    Foreground = new SolidColorBrush(Colors.Gray),
                    FontSize = 12,
                    Margin = new Thickness(0, 0, 0, 5)
                });
            }

            // 添加代码内容
            panel.Children.Add(new TextBlock
            {
                Text = code.Trim(),
                FontFamily = new FontFamily("Consolas, Menlo, Monaco, 'Courier New', monospace"),
                TextWrapping = TextWrapping.Wrap
            });

            border.Child = panel;

            return border;
        }

        private static Control CreateHorizontalRule()
        {
            return new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Colors.LightGray),
                Margin = new Thickness(0, 10, 0, 10)
            };
        }

        private static InlineCollection ParseInlineFormatting(string text)
        {
            var inlines = new InlineCollection();

            // 临时存储当前文本
            var currentText = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                // 处理粗体 (**text**)
                if (c == '*' && i + 1 < text.Length && text[i + 1] == '*')
                {
                    // 添加当前普通文本
                    if (currentText.Length > 0)
                    {
                        inlines.Add(new Run { Text = currentText.ToString() });
                        currentText.Clear();
                    }

                    i += 2; // 跳过**
                    int startPos = i;

                    while (i < text.Length && !(text[i] == '*' && i + 1 < text.Length && text[i + 1] == '*'))
                    {
                        i++;
                    }

                    if (i < text.Length)
                    {
                        string boldText = text.Substring(startPos, i - startPos);
                        inlines.Add(new Run { Text = boldText, FontWeight = FontWeight.Bold });
                        i++; // 跳过第二个*
                    }
                    else
                    {
                        // 如果没有找到结束标记，回退
                        i = startPos - 2;
                        currentText.Append(c);
                    }
                }
                // 处理斜体 (*text*)
                else if (c == '*' && (i == 0 || text[i - 1] != '*'))
                {
                    // 添加当前普通文本
                    if (currentText.Length > 0)
                    {
                        inlines.Add(new Run { Text = currentText.ToString() });
                        currentText.Clear();
                    }

                    i++; // 跳过*
                    int startPos = i;

                    while (i < text.Length && text[i] != '*')
                    {
                        i++;
                    }

                    if (i < text.Length)
                    {
                        string italicText = text.Substring(startPos, i - startPos);
                        inlines.Add(new Run { Text = italicText, FontStyle = FontStyle.Italic });
                    }
                    else
                    {
                        // 如果没有找到结束标记，回退
                        i = startPos - 1;
                        currentText.Append(c);
                    }
                }
                // 处理内联代码 (`code`)
                else if (c == '`')
                {
                    // 添加当前普通文本
                    if (currentText.Length > 0)
                    {
                        inlines.Add(new Run { Text = currentText.ToString() });
                        currentText.Clear();
                    }

                    i++; // 跳过`
                    int startPos = i;

                    while (i < text.Length && text[i] != '`')
                    {
                        i++;
                    }

                    if (i < text.Length)
                    {
                        string codeText = text.Substring(startPos, i - startPos);
                        var codeRun = new Run
                        {
                            Text = codeText,
                            FontFamily = new FontFamily("Consolas, Menlo, Monaco, 'Courier New', monospace"),
                            Background = new SolidColorBrush(Color.FromRgb(240, 240, 240))
                        };
                        inlines.Add(codeRun);
                    }
                    else
                    {
                        // 如果没有找到结束标记，回退
                        i = startPos - 1;
                        currentText.Append(c);
                    }
                }
                // 处理链接 [text](url)
                else if (c == '[')
                {
                    // 添加当前普通文本
                    if (currentText.Length > 0)
                    {
                        inlines.Add(new Run { Text = currentText.ToString() });
                        currentText.Clear();
                    }

                    i++; // 跳过[
                    int linkTextStart = i;

                    while (i < text.Length && text[i] != ']')
                    {
                        i++;
                    }

                    if (i < text.Length && i + 1 < text.Length && text[i + 1] == '(')
                    {
                        string linkText = text.Substring(linkTextStart, i - linkTextStart);
                        i += 2; // 跳过](
                        int urlStart = i;

                        while (i < text.Length && text[i] != ')')
                        {
                            i++;
                        }

                        if (i < text.Length)
                        {
                            string url = text.Substring(urlStart, i - urlStart);

                            var hyperlink = new Run
                            {
                                Text = linkText,
                                Foreground = new SolidColorBrush(Colors.Blue),
                                TextDecorations = TextDecorations.Underline
                            };

                            // 在Avalonia中我们不能直接添加链接点击行为
                            // 需要在父级控件上处理点击事件
                            inlines.Add(hyperlink);
                        }
                        else
                        {
                            // 如果没有找到结束标记，回退
                            i = linkTextStart - 1;
                            currentText.Append(c);
                        }
                    }
                    else
                    {
                        // 如果没有找到结束标记，回退
                        i = linkTextStart - 1;
                        currentText.Append(c);
                    }
                }
                else
                {
                    // 普通文本
                    currentText.Append(c);
                }
            }

            // 添加剩余的普通文本
            if (currentText.Length > 0)
            {
                inlines.Add(new Run { Text = currentText.ToString() });
            }

            return inlines;
        }
    }
}
