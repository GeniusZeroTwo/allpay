import { resolve } from "node:path";

process.env.NODE_ENV = "test";
process.env.DATA_DIR = resolve("./.test-data", `test-${process.pid}`);
process.env.APP_MASTER_KEY = "BwcHBwcHBwcHBwcHBwcHBwcHBwcHBwcHBwcHBwcHBwc=";
process.env.PUBLIC_BASE_URL = "http://localhost";

