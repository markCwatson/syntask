import * as vscode from 'vscode';
import { RoslynClient } from './roslynClient';

export function activate(context: vscode.ExtensionContext) {
  const client = new RoslynClient(context);
  context.subscriptions.push(client);

  const hoverProvider = vscode.languages.registerHoverProvider(
    { language: 'csharp' },
    {
      async provideHover(document, position) {
        const config = vscode.workspace.getConfiguration('syntask');

        if (!config.get<boolean>('enabled')) {
          return undefined;
        }

        if (config.get<string>('triggerMode') === 'commandOnly') {
          return undefined;
        }

        // Try diagnostic explanation first — errors take priority over syntax learning
        // Only trigger when VS Code itself shows a squiggle at this position
        if (config.get<boolean>('diagnosticExplanations.enabled', true)) {
          const editorDiagnostics = vscode.languages
            .getDiagnostics(document.uri)
            .filter(
              (d) =>
                d.range.contains(position) &&
                (d.severity === vscode.DiagnosticSeverity.Error ||
                  d.severity === vscode.DiagnosticSeverity.Warning),
            );

          if (editorDiagnostics.length > 0) {
            let diagResponse;
            try {
              diagResponse = await client.getDiagnosticExplanation({
                text: document.getText(),
                filePath:
                  document.uri.scheme === 'file' ? document.uri.fsPath : null,
                line: position.line,
                character: position.character,
                detailLevel: config.get<string>('detailLevel') ?? 'beginner',
                includeExamples: config.get<boolean>('includeExamples') ?? true,
              });
            } catch {
              // fall through to syntax hover
            }

            if (
              diagResponse?.markdown &&
              !diagResponse.markdown.includes(
                'No compiler diagnostic found at this position',
              )
            ) {
              const markdown = new vscode.MarkdownString(diagResponse.markdown);
              markdown.supportHtml = false;
              markdown.isTrusted = false;

              const range = diagResponse.range
                ? new vscode.Range(
                    diagResponse.range.startLine,
                    diagResponse.range.startCharacter,
                    diagResponse.range.endLine,
                    diagResponse.range.endCharacter,
                  )
                : document.getWordRangeAtPosition(position);

              return new vscode.Hover(markdown, range);
            }
          }
        }

        // No diagnostic — try syntax explanation
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

        if (response?.markdown) {
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
        }

        return undefined;
      },
    },
  );

  const syntaxCommand = vscode.commands.registerCommand(
    'syntask.explainSyntaxAtCursor',
    async () => {
      const editor = vscode.window.activeTextEditor;

      if (!editor || editor.document.languageId !== 'csharp') {
        vscode.window.showInformationMessage('Open a C# file first.');
        return;
      }

      const config = vscode.workspace.getConfiguration('syntask');

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

  const diagnosticCommand = vscode.commands.registerCommand(
    'syntask.explainDiagnosticAtCursor',
    async () => {
      const editor = vscode.window.activeTextEditor;

      if (!editor || editor.document.languageId !== 'csharp') {
        vscode.window.showInformationMessage('Open a C# file first.');
        return;
      }

      const config = vscode.workspace.getConfiguration('syntask');

      if (!config.get<boolean>('diagnosticExplanations.enabled', true)) {
        return;
      }

      let response;
      try {
        response = await client.getDiagnosticExplanation({
          text: editor.document.getText(),
          filePath:
            editor.document.uri.scheme === 'file'
              ? editor.document.uri.fsPath
              : null,
          line: editor.selection.active.line,
          character: editor.selection.active.character,
          detailLevel: config.get<string>('detailLevel') ?? 'beginner',
          includeExamples: config.get<boolean>('includeExamples') ?? true,
        });
      } catch {
        vscode.window.showErrorMessage('Roslyn server request failed.');
        return;
      }

      if (!response?.markdown) {
        vscode.window.showInformationMessage(
          'No compiler diagnostic found here.',
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

  context.subscriptions.push(hoverProvider, syntaxCommand, diagnosticCommand);
}

export function deactivate() {}
