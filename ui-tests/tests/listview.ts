import { Selector } from "testcafe"
import { authenticateIfNecessary } from "./ms-login"
import { config } from "../config"
import { query } from "../query"

fixture("Listview fixture")
  // .page("https://localhost:5002/list-view")
  .page(config.URL + "/list-view")
  .beforeEach(async (t) => {
    await authenticateIfNecessary()
  })

interface ListViewItem {
  id: string
  displayName: string
  birthday: string
  city: string
}

test("Search in Listview", async (t) => {
  const searchString = "ba"
  const data = await query<ListViewItem>()("/api/list-view/query", {
    search: searchString,
    skip: 0,
    take: 10,
  })
  console.log(JSON.stringify(data.value, null, 2))

  await t
    .typeText(
      Selector("input").withAttribute("placeholder", "search"),
      searchString,
    )
    .expect(Selector("span").withText("Vanessa.Kilback10").exists)
    .eql(true)
    .expect(Selector("span").withText(data.value[1].displayName).exists)
    .eql(true)
    //countertest
    .expect(Selector("span").withText("asdasdasdas").exists)
    .eql(false)
    .click(Selector(".ant-table-column-sorters").withExactText("Name"))
    .expect(Selector("span").withText("Eddie.Nikolaus").exists)
    .eql(true)
    .expect(Selector("span").withText(data.value[1].displayName).exists)
    .eql(false)
})
