import { t } from "testcafe"
import { Selector } from "testcafe"
import { config } from "../config"

export async function authenticateIfNecessary() {
  if (await Selector("button").withText("LOGIN").exists) {
    await t.click(Selector("button").withText("LOGIN"))
    // sometimes the first click does not work
    if (await Selector("button").withText("LOGIN").exists) {
      await t.click(Selector("button").withText("LOGIN"))
    }
    await authenticate()
  }
}

// logs the configured user in
// the ms login dialog starting with email/username must be shown
export async function authenticate() {
  await t.typeText(
    Selector("input").withAttribute("placeholder", /Email/),
    config.UserName,
  )

  await t
    .click(Selector("input").withAttribute("value", "Next"))
    .typeText(
      Selector("input").withAttribute("placeholder", "Password"),
      config.Password,
    )
    .click(Selector("input").withAttribute("value", "Sign in"))

  if (await Selector("div").withText("More information required").exists) {
    await t.click(
      Selector("a").withText("Skip for now (14 days until this is required)"),
    )
  }

  if (await Selector("div").withText("Permissions requested").exists) {
    await t.click(Selector("input").withAttribute("value", "Accept"))
  }

  if (await Selector("div").withText("Stay signed in?").exists) {
    await t.click(Selector("input").withAttribute("value", "Yes"))
  }
}
