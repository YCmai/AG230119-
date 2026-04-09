<template>
  <a-modal
    :visible="true"
    :width="620"
    :title="$t('title.form')"
    class="default-hb-modal"
  >
    <a-form-model
      :model="areaForm"
      ref="areaForm"
      layout="inline"
      :rules="rules"
      class="ant-form-lable-100"
    >
      <a-form-model-item prop="wareHouseId" label="库别编码">
        <a-input :disabled="true" v-model="areaForm.wareHouseId"></a-input>
      </a-form-model-item>
      <a-form-model-item prop="wareHouseName" label="库别名称">
        <a-input :disabled="true" v-model="areaForm.wareHouseName"></a-input>
      </a-form-model-item>
      <a-form-model-item prop="code" label="区域编码">
        <a-input v-model="areaForm.code" :disabled="isEdit"></a-input>
      </a-form-model-item>
      <a-form-model-item prop="name" label="区域名称">
        <a-input v-model="areaForm.name"></a-input>
      </a-form-model-item>
      <a-form-model-item prop="category" label="区域分类">
        <a-input v-model="areaForm.category"></a-input>
      </a-form-model-item>
      <a-form-model-item prop="description" label="区域描述" class="ant-form-item-100">
        <a-textarea v-model="areaForm.description"></a-textarea>
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
import locale from "@/localize/warehouse/area";
import { CreateArea, UpdateArea, GetArea } from "@/api/inventory/area";
export default {
  name: "AreaForm",
  i18n: {
    messages: locale,
  },
  data() {
    return {
      loading: false,
      isEdit: false,
      areaForm: {
        wareHouseId: "",
        wareHouseName: "",
        code: "",
        name: "",
        description: "",
        category: "",
      },
      rules: {
        wareHouseId: [{ required: true, message: "库别编码不能为空!" }],
        code: [{ required: true, message: "区域编码不能为空!", trigger: "blur" }],
        name: [{ required: true, message: "区域名称不能为空!", trigger: "blur" }],
      },
    };
  },
  methods: {
    load: async function (code, wareHouse = {}) {
      this.areaForm = Object.assign(this.areaForm, {
        wareHouseId: wareHouse.id,
        wareHouseName: wareHouse.name,
      });
      if (code) {
        this.isEdit = true;
        this.loading = true;
        try {
          let area = await GetArea(wareHouse.id, code);
          this.areaForm = Object.assign(this.areaForm, area);
        } catch (err) {
        } finally {
          this.loading = false;
        }
      }
    },
    save: async function () {
      if (!(await this.valid())) return;
      this.loading = true;
      try {
        if (!this.isEdit) await CreateArea(this.areaForm);
        else
          await UpdateArea(this.areaForm.wareHouseId, this.areaForm.code, {
            name: this.areaForm.name,
            category: this.areaForm.category,
            description: this.areaForm.description,
          });
        this.$message.success("保存成功!");
        this.close();
      } catch (err) {
        this.$message.error(err.message);
      } finally {
        this.loading = false;
      }
    },
    close: function () {
      this.$emit("close");
    },
    valid: function () {
      return new Promise((resolve) => {
        this.$refs.areaForm.validate((valid) => resolve(valid));
      });
    },
  },
};
</script>
