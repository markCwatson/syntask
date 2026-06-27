import * as cp from 'child_process';
import * as path from 'path';
import * as readline from 'readline';
import * as vscode from 'vscode';

type PendingRequest = {
  resolve: (value: any) => void;
  reject: (error: Error) => void;
};

export class RoslynClient implements vscode.Disposable {
  private process: cp.ChildProcessWithoutNullStreams | undefined;
  private nextId = 1;
  private pending = new Map<number, PendingRequest>();
  private output: vscode.OutputChannel;

  constructor(private readonly context: vscode.ExtensionContext) {
    this.output = vscode.window.createOutputChannel('syntask');
  }

  async getHover(params: any): Promise<any | undefined> {
    this.ensureStarted();

    const child = this.process;
    if (!child) {
      return undefined;
    }

    const id = this.nextId++;

    const message = {
      id,
      method: 'hover',
      params,
    };

    child.stdin.write(JSON.stringify(message) + '\n');

    return new Promise((resolve, reject) => {
      this.pending.set(id, { resolve, reject });

      setTimeout(() => {
        if (this.pending.delete(id)) {
          reject(new Error('Roslyn hover request timed out.'));
        }
      }, 5000);
    });
  }

  async getDiagnosticExplanation(params: any): Promise<any | undefined> {
    this.ensureStarted();

    const child = this.process;
    if (!child) {
      return undefined;
    }

    const id = this.nextId++;

    const message = {
      id,
      method: 'diagnosticExplanation',
      params,
    };

    child.stdin.write(JSON.stringify(message) + '\n');

    return new Promise((resolve, reject) => {
      this.pending.set(id, { resolve, reject });

      setTimeout(() => {
        if (this.pending.delete(id)) {
          reject(new Error('Roslyn diagnostic explanation request timed out.'));
        }
      }, 5000);
    });
  }

  private ensureStarted(): void {
    if (this.process) {
      return;
    }

    const serverDir = path.join(this.context.extensionPath, 'out', 'server');

    // Platform-specific self-contained binary, with dotnet fallback for dev
    const platform = process.platform;
    const arch = process.arch;

    let binaryName = 'CSharpLearningServer';
    if (platform === 'win32') {
      binaryName += '.exe';
    }

    const nativePath = path.join(serverDir, binaryName);
    const dllPath = path.join(serverDir, 'CSharpLearningServer.dll');

    let command: string;
    let args: string[];

    try {
      require('fs').accessSync(nativePath, require('fs').constants.X_OK);
      command = nativePath;
      args = [];
      this.output.appendLine(`Starting native server: ${nativePath}`);
    } catch {
      command = 'dotnet';
      args = [dllPath];
      this.output.appendLine(
        `Starting framework-dependent server: dotnet ${dllPath}`,
      );
    }

    this.process = cp.spawn(command, args, {
      cwd: this.context.extensionPath,
    });

    this.process.stderr.on('data', (data) => {
      this.output.appendLine(String(data));
    });

    const reader = readline.createInterface({
      input: this.process.stdout,
    });

    reader.on('line', (line) => {
      try {
        const response = JSON.parse(line);
        const pending = this.pending.get(response.id);

        if (!pending) {
          return;
        }

        this.pending.delete(response.id);

        if (response.error) {
          pending.reject(new Error(response.error));
        } else {
          pending.resolve(response.result);
        }
      } catch (err) {
        this.output.appendLine(`Failed to parse server response: ${line}`);
      }
    });

    this.process.on('exit', (code) => {
      this.output.appendLine(`Roslyn server exited with code ${code}.`);
      this.process = undefined;
    });
  }

  dispose(): void {
    this.process?.kill();
    this.output.dispose();
  }
}
