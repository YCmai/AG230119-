<template>
  <div style="display: inline">
    <a-dropdown>
      <a class="ant-dropdown-link" @click="(e) => e.preventDefault()">
        {{ current }}<a-icon type="down" />
      </a>
      <a-menu slot="overlay">
        <a-menu-item>
          <a href="javascript:;" @click="change('en')">English</a>
        </a-menu-item>
        <a-menu-item>
          <a href="javascript:;" @click="change('zh-Hans')">简体中文</a>
        </a-menu-item>
      </a-menu>
    </a-dropdown>
  </div>
</template>
<script>
//const { component } = require("lib");
// import store from "@/lib/global-store";
import component from "@/lib/base";
import { SetCulture } from "@/api/app";
import locale from "@/localize/locale";
export default {
  name: "Culture",
  mixins: [component],
  i18n: {
    messages: locale,
  },
  computed: {
    current() {
      let culture = this.$store.getters["global/getCulture"]; //store.getGlobalState("culture");
      if (!culture) return "简体中文";
      if (culture == "zh-Hans") {
        return "简体中文";
      } else return "English";
    },
  },
  methods: {
    change: async function (language) {
      let _this = this;
      try {
        let result = await _this.confirm(_this.$t("message.language"));
        if (!result) return;
        await SetCulture(language);
        location.reload();
      } catch (err) {
        _this.$message.error(err.message);
      }
    },
  },
};
</script>
