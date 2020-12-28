import * as dotenv from 'dotenv';

dotenv.config({ path: '.env.test' });

export const config = {
  URL: process.env.TEST_URL,
  UserName: process.env.TEST_USERNAME,
  Password: process.env.TEST_PASSWORD,
};

const notFound = [];
if (!config.URL) {
  notFound.push('TEST_URL');
}
if (!config.UserName) {
  notFound.push('TEST_USERNAME');
}
if (!config.Password) {
  notFound.push('TEST_PASSWORD');
}
if (!config.URL || !config.UserName || !config.Password) {
  // If you are running tests locally, provide a .env file in this projects root directory and fill it with environment variables
  // i.e.
  // TEST_URL=https://gertrud.com/
  // TEST_USERNAME=username@company.com
  // TEST_PASSWORD=some_password
  // Do not commit the .env file to the repository
  // If you are creating a CI pipeline provide these configuration values as environment variables
  console.log(JSON.stringify(config));
  const error = `Could not read required environment variables. Pls provide ${JSON.stringify(
    notFound
  )} `;
  throw new Error(error);
}
