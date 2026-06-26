import * as vscode from 'vscode';
import { RoslynClient } from './roslynClient';

export function activate(context: vscode.ExtensionContext) {
  const client = new RoslynClient(context);
  context.subscriptions.push(client);

  const hoverProvider = vscode.languages.registerHoverProvider(
    { language: 'csharp' },
    {
      async provideHover(document, position) {
        const config = vscode.workspace.getConfiguration(
          'csharpLearningHovers',
        );

        if (!config.get<boolean>('enabled')) {
          return undefined;
        }

        if (config.get<string>('triggerMode') === 'commandOnly') {
          return undefined;
        }

        let response;
        try {
          response = await client.getHover({
            text: document.getText(),
            filePath:
              document.uri.scheme === 'file' ? document.uri.fsPath : null,
            line: position.line,
            character: position.character,
            detailLevel: config.get<string>('detailLevel') ?? 'beginner',
            includeExamples: config.get<boolean>('includeExamples') ?? true,
          });
        } catch {
          return undefined;
        }

        if (!response?.markdown) {
          return undefined;
        }

        const markdown = new vscode.MarkdownString(response.markdown);
        markdown.supportHtml = false;
        markdown.isTrusted = false;

        const range = response.range
          ? new vscode.Range(
              response.range.startLine,
              response.range.startCharacter,
              response.range.endLine,
              response.range.endCharacter,
            )
          : document.getWordRangeAtPosition(position);

        return new vscode.Hover(markdown, range);
      },
    },
  );

  context.subscriptions.push(hoverProvider);

  const command = vscode.commands.registerCommand(
    'csharpLearningHovers.explainSyntaxAtCursor',
    async () => {
      const editor = vscode.window.activeTextEditor;

      if (!editor || editor.document.languageId !== 'csharp') {
        vscode.window.showInformationMessage('Open a C# file first.');
        return;
      }

      const config = vscode.workspace.getConfiguration('csharpLearningHovers');

      let response;
      try {
        response = await client.getHover({
          text: editor.document.getText(),
          filePath:
            editor.document.uri.scheme === 'file'
              ? editor.document.uri.fsPath
              : null,
          line: editor.selection.active.line,
          character: editor.selection.active.character,
          detailLevel: config.get<string>('detailLevel') ?? 'beginner',
          includeExamples: true,
        });
      } catch {
        vscode.window.showErrorMessage('Roslyn server request failed.');
        return;
      }

      if (!response?.markdown) {
        vscode.window.showInformationMessage(
          'No C# syntax explanation found here.',
        );
        return;
      }

      const doc = await vscode.workspace.openTextDocument({
        content: response.markdown,
        language: 'markdown',
      });

      await vscode.window.showTextDocument(doc, { preview: true });
    },
  );

  context.subscriptions.push(command);
}

export function deactivate() {}
