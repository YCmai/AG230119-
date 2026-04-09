<template>
  <a-modal
    ref="dialog"
    :width="650"
    :title="$t('title.title')"
    :visible="true"
    class="default-hb-modal"
  >
    <a-form-model :model="form" ref="settingForm" layout="inline">
      <a-form-model-item
        :label="$t('title.displayName')"
        prop="displayName"
        class="hb-form-col-2"
      >
        <a-input
          v-model="form.displayName"
          :placeholder="$t('placeholder.displayName')"
          disabled
        >
        </a-input>
      </a-form-model-item>
      <a-form-model-item
        :label="$t('title.value')"
        prop="currentValue"
        class="hb-form-col-2"
      >
        <a-input v-model="form.currentValue" :placeholder="$t('placeholder.value')">
        </a-input>
      </a-form-model-item>
      <a-form-model-item
        :label="$t('title.description')"
        prop="description"
        class="hb-form-col-2"
      >
        <a-textarea
          style="width: 465px"
          disabled
          v-model="form.description"
          :placeholder="$t('placeholder.description')"
        >
        </a-textarea>
      </a-form-model-item>
    </a-form-model>
    <template slot="footer">
      <a-button key="back" @click="close" size="small">
        {{ $t("actions.cancel") }}
      </a-button>
      <a-button key="submit" type="primary" :loading="loading" @click="save" size="small">
        {{ $t("actions.save") }}
      </a-button>
    </template>
  </a-modal>
</template>
<script>
import component from "@/lib/base";
import { Save } from "@/api/setting";
import locale from "@/localize/system/settings";
export default {
  name: "SettingForm",
  mixins: [component],
  i18n: {
    messages: locale,
  },
  data() {
    return {
      loading: false,
      form: {
        displayName: "",
        currentValue: "",
        description: "",
      },
    };
  },
  methods: {
    save: async function () {
      let _this = this;
      debugger;
      if (!(await this.confirm(this.$t("confirm.save")))) return;
      try {
        this.loading = true;
        await Save({ name: _this.form.name, value: _this.form.currentValue });
        this.$message.success(_this.$t("save.success"));
        this.close();
      } catch (err) {
      } finally {
        this.loading = false;
      }
    },
    load: function (data) {
      this.form = Object.assign(this.form, data);
    },
    close: function () {
      this.$emit("close");
    },
  },
};
</script>
