import { ClientFunction } from 'testcafe';

export const getPageUrl = ClientFunction(() => window.location.href);

export const GuidRegEx = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

export const ContainsGuidRegEx = /(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}/i;

export async function write(t: TestController, text: string) {
  for (let i = 0; i < text.length; i++) {
    await t.pressKey(text[i]);
  }
}
